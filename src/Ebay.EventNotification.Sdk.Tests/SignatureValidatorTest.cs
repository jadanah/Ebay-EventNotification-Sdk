using System.Threading.Tasks;
using Ebay.EventNotification.Sdk.Client;
using Ebay.EventNotification.Sdk.Tests.data;
using Ebay.EventNotification.Sdk.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Ebay.EventNotification.Sdk.Tests
{
    public class SignatureValidatorTest
    {
        private readonly IPublicKeyCache _publicKeyCache;
        private readonly IMessageSerializer _serializer;

        public SignatureValidatorTest()
        {
            var publicKeyClientMock = new Mock<IPublicKeyClient>();
            publicKeyClientMock.Setup(x => x.GetPublicKeyAsync(It.IsAny<string>())).Returns(() => Task.FromResult(DataProvider.GetMockPublicKeyResponse()));

            _publicKeyCache = new PublicKeyCache(publicKeyClientMock.Object, new MemoryCache(new MemoryCacheOptions()));
            _serializer = new MessageSerializer();
        }


#if NET5_0_OR_GREATER
        [Fact]
        public async Task Should_validate_from_raw_message_using_dotnet()
        {
            var validator = new DotnetSignatureValidator(_publicKeyCache, _serializer, new NullLogger<DotnetSignatureValidator>());

            var message = DataProvider.GetMockMessageRaw();
            var signature = DataProvider.GetMockXEbaySignatureHeader();
            var valid = await validator.ValidateAsync(message, signature);

            Assert.True(valid);
        }

        [Fact]
        public async Task Should_validate_from_message_using_dotnet()
        {
            var validator = new DotnetSignatureValidator(_publicKeyCache, _serializer, new NullLogger<DotnetSignatureValidator>());

            var message = DataProvider.GetMockMessage();
            var signature = DataProvider.GetMockXEbaySignatureHeader();
            var valid = await validator.ValidateAsync(message, signature);

            Assert.True(valid);
        }
        
        [Fact]
        public async Task Should_fail_from_tampered_raw_message_using_dotnet()
        {
            var validator = new DotnetSignatureValidator(_publicKeyCache, _serializer, new NullLogger<DotnetSignatureValidator>());

            var message = DataProvider.GetMockTamperedMessageRaw();
            var signature = DataProvider.GetMockXEbaySignatureHeader();
            var valid = await validator.ValidateAsync(message, signature);

            Assert.False(valid);
        }

        [Fact]
        public async Task Should_fail_from_tampered_message_using_dotnet()
        {
            var validator = new DotnetSignatureValidator(_publicKeyCache, _serializer, new NullLogger<DotnetSignatureValidator>());

            var message = DataProvider.GetMockTamperedMessage();
            var signature = DataProvider.GetMockXEbaySignatureHeader();
            var valid = await validator.ValidateAsync(message, signature);

            Assert.False(valid);
        }
#else
        [Fact]
        public async Task Should_validate_from_raw_message_using_bc()
        {
            var validator = new BouncyCastleSignatureValidator(_publicKeyCache, _serializer, new NullLogger<BouncyCastleSignatureValidator>());

            var message = DataProvider.GetMockMessageRaw();
            var signature = DataProvider.GetMockXEbaySignatureHeader();
            var valid = await validator.ValidateAsync(message, signature);

            Assert.True(valid);
        }

        [Fact]
        public async Task Should_validate_from_message_using_bc()
        {
            var validator = new BouncyCastleSignatureValidator(_publicKeyCache, _serializer, new NullLogger<BouncyCastleSignatureValidator>());

            var message = DataProvider.GetMockMessage();
            var signature = DataProvider.GetMockXEbaySignatureHeader();
            var valid = await validator.ValidateAsync(message, signature);

            Assert.True(valid);
        }

        [Fact]
        public async Task Should_fail_from_tampered_raw_message_using_bc()
        {
            var validator = new BouncyCastleSignatureValidator(_publicKeyCache, _serializer, new NullLogger<BouncyCastleSignatureValidator>());

            var message = DataProvider.GetMockTamperedMessageRaw();
            var signature = DataProvider.GetMockXEbaySignatureHeader();
            var valid = await validator.ValidateAsync(message, signature);

            Assert.False(valid);
        }

        [Fact]
        public async Task Should_fail_from_tampered_message_using_bc()
        {
            var validator = new BouncyCastleSignatureValidator(_publicKeyCache, _serializer, new NullLogger<BouncyCastleSignatureValidator>());

            var message = DataProvider.GetMockTamperedMessage();
            var signature = DataProvider.GetMockXEbaySignatureHeader();
            var valid = await validator.ValidateAsync(message, signature);

            Assert.False(valid);
        }
#endif
    }
}