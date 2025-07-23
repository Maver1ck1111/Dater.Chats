using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Domain
{
    public class Chat
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid ID { get; set; }

        [BsonElement("usersId")]
        public List<Guid> UsersId { get; set; } = new List<Guid>();

        [BsonElement("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
