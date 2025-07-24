using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Application.ServicesContracts
{
    public interface IEncryptionService
    {
        public string Encrypt(string input);
        public string Decrypt(string input);
    }
}
