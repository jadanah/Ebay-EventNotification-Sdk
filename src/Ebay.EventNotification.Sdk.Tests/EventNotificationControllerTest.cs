using System.Net;
using System.Threading.Tasks;
using Ebay.EventNotification.Sdk.AspNetCore.Controllers;
using Ebay.EventNotification.Sdk.Client;
using Ebay.EventNotification.Sdk.Config;
using Ebay.EventNotification.Sdk.Constants;
using Ebay.EventNotification.Sdk.Processor;
using Ebay.EventNotification.Sdk.Tests.data;
using Ebay.EventNotification.Sdk.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Ebay.EventNotification.Sdk.Tests
{
    public class EventNotificationControllerTest
    {
        private readonly EventNotificationController _controller;

        private readonly Mock<ILogger<EventNotificationController>> _logger = new Mock<ILogger<EventNotificationController>>();

        private readonly Mock<IPublicKeyClient> _publicKeyClientMock = new Mock<IPublicKeyClient>();

        private readonly Mock<IMessageProcessorFactory> _messageProcessorFactoryMock = new Mock<IMessageProcessorFactory>();

        private readonly Mock<IEventNotificationConfig> _configMock = new Mock<IEventNotificationConfig>();

        

        public EventNotificationControllerTest()
        {
            IPublicKeyCache publicKeyCache = new PublicKeyCache(_publicKeyClientMock.Object, new MemoryCache(new MemoryCacheOptions()));

#if NET5_0_OR_GREATER
            ISignatureValidator signatureValidator = new DotnetSignatureValidator(publicKeyCache, new MessageSerializer(),new NullLogger<DotnetSignatureValidator>() );
#else
            ISignatureValidator signatureValidator = new BouncyCastleSignatureValidator(publicKeyCache, new MessageSerializer(),new NullLogger<BouncyCastleSignatureValidator>() );
#endif
            
            
            IEndPointValidator endpointValidator = new EndpointValidator(_configMock.Object);

            _publicKeyClientMock.Setup(x => x.GetPublicKeyAsync(It.IsAny<string>())).Returns(() => Task.FromResult(DataProvider.GetMockPublicKeyResponse()));

            _messageProcessorFactoryMock.Setup(x => x.GetProcessor(It.IsAny<TopicEnum>()))
                .Returns(() => new ExampleAccountDeletionMessageProcessor(new Mock<ILogger<ExampleAccountDeletionMessageProcessor>>().Object));

            _controller = new EventNotificationController(_logger.Object, signatureValidator, endpointValidator, _messageProcessorFactoryMock.Object);
        }

        [Fact]
        public async Task TestPayloadProcessingSuccess()
        {
            IActionResult result = await _controller.Process(DataProvider.GetMockMessage(), DataProvider.GetMockXEbaySignatureHeader());

            var objectResponse = Assert.IsType<NoContentResult>((ActionResult)result);
            Assert.Equal((int)HttpStatusCode.NoContent, objectResponse.StatusCode);
        }

        [Fact]
        public async Task TestPayLoadVerificationFailure()
        {
            IActionResult result = await _controller.Process(DataProvider.GetMockTamperedMessage(), DataProvider.GetMockXEbaySignatureHeader());

            var objectResponse = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((int)HttpStatusCode.PreconditionFailed, objectResponse.StatusCode);
        }

        [Fact]
        public void TestVerification()
        {
            var challengeCode = "a8628072-3d33-45ee-9004-bee86830a22d";
            var verificationToken = "71745723-d031-455c-bfa5-f90d11b4f20a";
            var endpoint = "http://www.testendpoint.com/webhook";
            var expectedResult = "ca527df75caa230092d7e90484071e8f05d63068f1317973d6a3a42593734bbb";

            _configMock.Setup(x => x.Endpoint).Returns(endpoint);
            _configMock.Setup(x => x.VerificationToken).Returns(verificationToken);
            var result = _controller.Validate(challengeCode);

            Assert.Equal(expectedResult, result.Value.Response);
        }
    }
}