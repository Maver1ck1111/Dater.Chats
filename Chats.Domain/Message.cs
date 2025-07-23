using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Domain
{
    public class Message
    {
        [BsonId]
        public Guid ID { get; set; }

        [BsonElement("senderId")]
        public Guid SenderId { get; set; }

        [BsonElement("text")]
        public string Text { get; set; } = string.Empty;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
