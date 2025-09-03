using Chats.Application.RepositoryContracts;
using Chats.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Chats.WebApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private readonly ConcurrentDictionary<Guid, List<Message>> _chatActivity;
        public ChatHub(IChatRepository chatRepository, ConcurrentDictionary<Guid, List<Message>> chatActivity)
        {
            _chatRepository = chatRepository;
            _chatActivity = chatActivity;
        }
        public async Task JoinChat(string chatID)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatID);
            _chatActivity.TryAdd(Guid.Parse(chatID), new List<Message>());
        }

        public async Task LeaveChat(string chatID)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatID);

            if (_chatActivity.ContainsKey(Guid.Parse(chatID)) && _chatActivity[Guid.Parse(chatID)].Count > 0)
            {
                await _chatRepository.AddMessagesToChatAsync(Guid.Parse(chatID), _chatActivity[Guid.Parse(chatID)]);
                _chatActivity[Guid.Parse(chatID)].Clear();
            }
        }

        public async Task SendMessageToChat(string chatID, Message message)
        {
            await Clients.Group(chatID).SendAsync("ReceiveMessage", message);

            if (_chatActivity.ContainsKey(Guid.Parse(chatID)))
            {
                _chatActivity[Guid.Parse(chatID)].Add(message);
            }

            if(_chatActivity[Guid.Parse(chatID)].Count >= 10)
            {
                _chatRepository.AddMessagesToChatAsync(Guid.Parse(chatID), _chatActivity[Guid.Parse(chatID)]);
                _chatActivity[Guid.Parse(chatID)].Clear();
            }
        }
    }
}
