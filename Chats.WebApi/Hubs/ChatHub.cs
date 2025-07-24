using Chats.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chats.WebApi.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinChat(string chatID)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatID);
        }

        public async Task LeaveChat(string chatID)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatID);
        }

        public async Task SendMessageToChat(string chatID, Message message)
        {
            await Clients.Group(chatID).SendAsync("ReceiveMessage", message);
        }
    }
}
