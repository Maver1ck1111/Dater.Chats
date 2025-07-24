using Chats.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Tests
{
    public class EncryptorTests
    {
        private readonly Mock<ILogger<EncryptorService>> _loggerMock = new Mock<ILogger<EncryptorService>>();
        private readonly IConfiguration _configuration;
        public EncryptorTests()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "EncryptorKey", "12345678901234567890123456789012" },
                    { "EncryptorIv", "1234567890123456" }
               })
           .Build();

            _configuration = config;
        }

        [Fact]
        public void Encryptor_ShouldEncryptAndDecryptCorrectly()
        {
            var encryptor = new EncryptorService(_loggerMock.Object, _configuration);
            string originalText = "Hello World!";

            string encryptedText = encryptor.Encrypt(originalText);
            string decryptedText = encryptor.Decrypt(encryptedText);
            
            encryptedText.Should().NotBe(originalText);
            decryptedText.Should().Be(originalText);
        }
    }
}
