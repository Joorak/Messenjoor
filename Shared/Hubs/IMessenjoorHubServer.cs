using Messenjoor.Shared.Models;

namespace Messenjoor.Shared
{
    public interface IMessenjoorHubServer
    {
        Task SetUserOnline(UserModel user);
        Task SetUserOffline(UserModel user);
    }
}
