using Chats.Application.ServicesContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Application.Services
{
    public class EncryptorService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly ILogger<EncryptorService> _logger;
        public EncryptorService(ILogger<EncryptorService> logger, IConfiguration configuration)
        {
            _logger = logger;

            _key = Encoding.UTF8.GetBytes(configuration["EncryptorKey"]!);
            _iv = Encoding.UTF8.GetBytes(configuration["EncryptorIv"]!);
        }
        public string Decrypt(string input)
        {
            _logger.LogInformation("Reached Decrypt method of Encryptor service");

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(input));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        public string Encrypt(string input)
        {
            _logger.LogInformation("Reached Encrypt method of Encryptor service");

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(input);
                sw.Flush();
                cs.FlushFinalBlock();
            }

            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
