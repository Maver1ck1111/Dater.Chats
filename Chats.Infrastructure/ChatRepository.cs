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

        public async Task<Result> AddMessagesToChatAsync(Guid chatID, IEnumerable<Message> messages)
        {
            if(chatID == Guid.Empty)
            {
               _logger.LogError("Chat ID cannot be empty.");
               return Result.Failure(400, "Chat ID cannot be empty.");
            }

            var filter = Builders<Chat>.Filter.Eq(x => x.ID, chatID);
            var update = Builders<Chat>.Update.PushEach(x => x.Messages, messages);
            
            var result = await _chatsCollection.UpdateOneAsync(filter, update);

            if(result.MatchedCount == 0)
            {
                _logger.LogError("Chat not found with ID: {ChatID}", chatID);
                return Result.Failure(404, "Chat not found");
            }

            _logger.LogInformation("Messages added to chat with ID: {ChatID}", chatID);

            return Result.Success();
        }

        public async Task<Result<Guid>> CreateChatAsync(IEnumerable<Guid> usersID)
        {
            if(usersID == null || !usersID.Any() || usersID.Any(id => id == Guid.Empty))
            {
                _logger.LogError("Incorrect users ID");
                return Result<Guid>.Failure(400, "Incorrect users ID");
            }

            Chat chat = new Chat
            {
                ID = Guid.NewGuid(),
                UsersId = usersID.ToList(),
                Messages = new List<Message>()
            };

            await _chatsCollection.InsertOneAsync(chat);

            _logger.LogInformation("Chat created with ID: {ChatID}", chat.ID);
            return Result<Guid>.Success(chat.ID);
        }

        public async Task<Result> DeleteMessageAsync(Guid chatID, Guid messageID)
        {
            if(chatID == Guid.Empty || messageID == Guid.Empty)
            {
                _logger.LogError("Chat ID or Message ID cannot be empty.");
                return Result.Failure(400, "Chat ID or Message ID cannot be empty");
            }

            var filter = Builders<Chat>.Filter.Eq(x => x.ID, chatID);
            var update = Builders<Chat>.Update.PullFilter(x => x.Messages, m => m.ID == messageID);

            var result = await _chatsCollection.UpdateOneAsync(filter, update);

            if(result.MatchedCount == 0)
            {
                _logger.LogError("Chat not found with ID: {ChatID}", chatID);
                return Result.Failure(404, "Chat not found");
            }

            if(result.ModifiedCount == 0)
            {
                _logger.LogError("Message not found with ID: {MessageID}", messageID);
                return Result.Failure(404, "Message not found");
            }

            _logger.LogInformation("Message with ID: {MessageID} deleted from chat with ID: {ChatID}", messageID, chatID);
            return Result.Success();
        }

        public async Task<Result<IEnumerable<(Guid, Guid)>>> FindCompanionsChatsByUserAsync(Guid usersID)
        {
            if(usersID == Guid.Empty)
            {
                _logger.LogError("User ID cannot be empty.");
                return Result<IEnumerable<(Guid, Guid)>>.Failure(400, "User ID cannot be empty.");
            }

            var filter = Builders<Chat>.Filter.AnyEq(x => x.UsersId, usersID);

            var result = await (await _chatsCollection.FindAsync(filter)).ToListAsync();

            if(result.Count == 0)
            {
                _logger.LogInformation("No chats found for user with ID: {UserID}", usersID);
                return Result<IEnumerable<(Guid, Guid)>>.Failure(404, "Chats not found");
            }

            var companionsChats = result
                .SelectMany(chat => chat.UsersId
                        .Where(u => u != usersID)
                        .Select(companionID => (chat.ID, companionID)));

            int length = companionsChats.Count();

            _logger.LogInformation("Chats with userID: {UserID} founded", usersID);
            return Result<IEnumerable<(Guid, Guid)>>.Success(companionsChats);
        }

        public async Task<Result<IEnumerable<Message>>> GetMessagesFromChatAsync(Guid chatID, int skip, int take)
        {
            if(skip < 0 || take <= 0)
            {
                _logger.LogError("Skip and take parameters must be non-negative and take must be greater than zero.");
                return Result<IEnumerable<Message>>.Failure(400, "Invalid pagination parameters");
            }

            if(chatID == Guid.Empty)
            {
                _logger.LogError("Chat ID cannot be empty.");
                return Result<IEnumerable<Message>>.Failure(400, "Chat ID cannot be empty.");
            }

            var filter = Builders<Chat>.Filter.Eq(x => x.ID, chatID);

            var chat = await _chatsCollection.Find(filter).Project(c => c.Messages.Skip(skip).Take(take)).FirstOrDefaultAsync();

            if(chat == null)
            {
                _logger.LogError("Chat not found with ID: {ChatID}", chatID);
                return Result<IEnumerable<Message>>.Failure(404, "Chat not found");
            }

            _logger.LogInformation("Messages retrieved from chat with ID: {ChatID}", chatID);
            return Result<IEnumerable<Message>>.Success(chat);
        }

        public async Task<Result> UpdateMessageAsync(Guid chatID, Message message)
        {
            if(chatID == Guid.Empty)
            {
                _logger.LogError("Chat ID cannot be empty.");
                return Result.Failure(400, "Chat ID cannot be empty.");
            }

            if(message == null || message.ID == Guid.Empty)
            {
                _logger.LogError("Message cannot be null and must have a valid ID and content.");
                return Result.Failure(400, "Message cannot be null and must have a valid");
            }

            var chat = await (await _chatsCollection.FindAsync(c => c.ID == chatID)).FirstOrDefaultAsync();
            if (chat == null)
            {
                _logger.LogError("Chat with id: {ChatID} not found", chatID);
                return Result.Failure(404, "Chat not found");
            }

            var messageIndex = chat.Messages.FindIndex(m => m.ID == message.ID);
            if (messageIndex == -1)
            {
                _logger.LogError("Message with id: {MessageId} not found", message.ID);
                return Result.Failure(404, "Message not found");
            }

            chat.Messages[messageIndex] = message;

            await _chatsCollection.ReplaceOneAsync(c => c.ID == chatID, chat);

            _logger.LogInformation("Message with id: {MessageID} updated", message.ID);
            return Result.Success();
        }
    }
}
