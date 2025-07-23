using Chats.Application;
using Chats.Application.RepositoryContracts;
using Chats.Domain;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Infrastructure
{
    public class ChatRepository : IChatRepository
    {
        private readonly ILogger<ChatRepository> _logger;
        private readonly IMongoCollection<Chat> _chatsCollection;
        public ChatRepository(IMongoDatabase database, ILogger<ChatRepository> logger)
        {
            _logger = logger;
            _chatsCollection = database.GetCollection<Chat>("Chats");
        }

        public Task<Result> AddMessagesToChatAsync(Guid chatID, IEnumerable<Message> messages)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Guid>> CreateChatAsync(IEnumerable<Guid> usersID)
        {
            throw new NotImplementedException();
        }

        public Task<Result> DeleteMessageAsync(Guid chatID, Guid messageID)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Guid>> FindChatsByUserAsync(Guid usersID)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<Message>>> GetMessagesFromChatAsync(Guid chatID, int skip, int take)
        {
            throw new NotImplementedException();
        }

        public Task<Result> UpdateMessageAsync(Guid chatID, Message message)
        {
            throw new NotImplementedException();
        }
    }
}
