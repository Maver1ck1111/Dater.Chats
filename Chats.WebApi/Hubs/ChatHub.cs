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
        private readonly ConcurrentDictionary<Guid, ConcurrentBag<Message>> _chatActivity;
        public ChatHub(IChatRepository chatRepository, ConcurrentDictionary<Guid, ConcurrentBag<Message>> chatActivity)
        {
            _chatRepository = chatRepository;
            _chatActivity = chatActivity;
        }
        public async Task JoinChat(string chatID)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatID);
            _chatActivity.TryAdd(Guid.Parse(chatID), new ConcurrentBag<Message>());
        }

        public async Task LeaveChat(string chatID)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatID);

            var id = Guid.Parse(chatID);
            if (_chatActivity.TryGetValue(id, out var bag) && bag.Count > 0)
            {
                await _chatRepository.AddMessagesToChatAsync(id, bag);
                _chatActivity[id] = new ConcurrentBag<Message>();
            }
        }

        public async Task SendMessageToChat(string chatID, Message message)
        {
            await Clients.Group(chatID).SendAsync("ReceiveMessage", message);

            Guid id = Guid.Parse(chatID);

            if(_chatActivity.TryGetValue(id, out var bag))
            {
                bag.Add(message);

                if(bag.Count >= 10)
                {
                    await _chatRepository.AddMessagesToChatAsync(id, bag);
                    _chatActivity[id] = new ConcurrentBag<Message>();
                }
            }
        }
    }
}
