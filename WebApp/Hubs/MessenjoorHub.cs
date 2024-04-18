using Messenjoor.Controllers;
using Messenjoor.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Messenjoor.Hubs
{
    [Authorize]
    public class MessenjoorHub: Hub<IMessenjoorHubClient>, IMessenjoorHubServer
    {
        private readonly ILogger<AccountController> _logger;
        private static readonly IDictionary<int, UserModel> _onlineUsers = new Dictionary<int, UserModel>();


        public MessenjoorHub(ILogger<AccountController> logger)
        {
            _logger = logger;
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
                _logger.LogCritical($"{user.Name} logged off");
                try
                {
                    await Clients.Others.OnlineUsersList(_onlineUsers.Values);
                    _logger.LogCritical($"successful publishing onlineUsersList to hub after {user.Name} Logged off");
                }
                catch (Exception ex)
                {

                    _logger.LogCritical($"publishing onlineUsersList to hub after {user.Name} Logged off generate error : {ex.Message}");
                }
                
            }
        }

        public async Task SetUserOnline(UserModel user)
        {
            await Clients.Caller.OnlineUsersList(_onlineUsers.Values);
            if (!_onlineUsers.ContainsKey(user.Id))
            {
                _onlineUsers.Add(user.Id, user);
                _logger.LogCritical($"{user.Name} logged in");
                try
                {
                    await Clients.Others.UserIsOnline(user.Id);
                    _logger.LogCritical($"successful publishing onlineUsersList to hub after {user.Name} Logged off");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical($"publishing onlineUsersList to hub after {user.Name} Logged in generate error : {ex.Message}"); ;
                }
                
            }
        }
    }
}
