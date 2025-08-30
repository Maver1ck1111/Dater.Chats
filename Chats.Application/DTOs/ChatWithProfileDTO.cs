using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Application.DTOs
{
    public class ChatWithProfileDTO
    {
        public Guid ChatID { get; set; }
        public ProfileDTO Profile { get; set; } = null!;
    }
}
