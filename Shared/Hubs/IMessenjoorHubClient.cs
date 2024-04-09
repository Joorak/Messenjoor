using Messenjoor.Shared.Models;

 
namespace Messenjoor.Shared
{
    //Events that the user should be notified realtime, these events must be register on client hub,
    //because those events change the user UI such as when messege received or one contact loged out,
    //each user has seperate hub that interact server in it
    public interface IMessenjoorHubClient
    {
        Task UserConnected(UserModel user);
        Task OnlineUsersList(IEnumerable<UserModel> users);
        Task UserIsOnline(int userId);

        Task MessageRecieved(MessageModel messageDto);
    }
}
