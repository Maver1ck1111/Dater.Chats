using Chats.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Application.RepositoryContracts
{
    public interface IChatRepository
    {
        Task<Result<Guid>> FindChatsByUserAsync(Guid usersID);
        Task<Result<IEnumerable<Message>>> GetMessagesFromChatAsync(Guid chatID, int skip, int take);
        Task<Result<Guid>> CreateChatAsync(IEnumerable<Guid> usersID);
        Task<Result> AddMessagesToChatAsync(Guid chatID, IEnumerable<Message> messages);
        Task<Result> UpdateMessageAsync(Guid chatID, Message message);
        Task<Result> DeleteMessageAsync(Guid chatID, Guid messageID);
    }
}
