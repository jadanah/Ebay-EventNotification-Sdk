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
        private readonly ISignatureValidator _validator;

        public SignatureValidatorTest()
        {
            var publicKeyClientMock = new Mock<IPublicKeyClient>();
            publicKeyClientMock.Setup(x => x.GetPublicKeyAsync(It.IsAny<string>())).Returns(() => Task.FromResult(DataProvider.GetMockPublicKeyResponse()));
            
            IPublicKeyCache publicKeyCache = new PublicKeyCache(publicKeyClientMock.Object, new MemoryCache(new MemoryCacheOptions()));
            
            _validator = new SignatureValidator(publicKeyCache, new MessageSerializer(), new NullLogger<SignatureValidator>());
        }
        
        [Fact]
        public async Task Should_validate_from_raw_message()
        {
            var message = DataProvider.GetMockMessageRaw();
            var signature = DataProvider.GetMockXEbaySignatureHeader();

            var valid = await _validator.ValidateAsync(message, signature);

            Assert.True(valid);
        }
        
        [Fact]
        public async Task Should_validate_from_message()
        {
            var message = DataProvider.GetMockMessage();
            var signature = DataProvider.GetMockXEbaySignatureHeader();

            var valid = await _validator.ValidateAsync(message, signature);

            Assert.True(valid);
        }

        

    }
    
    
}