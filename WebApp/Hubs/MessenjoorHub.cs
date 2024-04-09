using Messenjoor.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Messenjoor.Hubs
{
    [Authorize]
    public class MessenjoorHub: Hub<IMessenjoorHubClient>, IMessenjoorHubServer
    {
        private static readonly IDictionary<int, UserModel> _onlineUsers = new Dictionary<int, UserModel>();

        public MessenjoorHub()
        {

        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SetUserOffline(UserModel? user)
        {
            if (user != null && _onlineUsers.ContainsKey(user.Id))
            {
                _onlineUsers.Remove(user.Id);
                await Clients.Others.OnlineUsersList(_onlineUsers.Values);
            }
        }

        public async Task SetUserOnline(UserModel user)
        {
            await Clients.Caller.OnlineUsersList(_onlineUsers.Values);
            if (!_onlineUsers.ContainsKey(user.Id))
            {
                _onlineUsers.Add(user.Id, user);
                await Clients.Others.UserIsOnline(user.Id);
            }
        }
    }
}
