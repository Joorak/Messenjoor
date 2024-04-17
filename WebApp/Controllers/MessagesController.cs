using Messenjoor.Shared;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebPush;

namespace Messenjoor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : BaseController
    {
        private readonly ChatContext _chatContext;
        private readonly IHubContext<MessenjoorHub, IMessenjoorHubClient> _hubContext;

        public MessagesController(ChatContext chatContext, IHubContext<MessenjoorHub, IMessenjoorHubClient> hubContext)
        {
            _chatContext = chatContext;
            _hubContext = hubContext;
        }

        
        // /messages
        [HttpPost("")]
        public async Task<IActionResult> SendMessage(MessageSendModel MessageModel, CancellationToken cancellationToken)
        {
            if (MessageModel.ToUserId <= 0 || string.IsNullOrWhiteSpace(MessageModel.Message))
                return BadRequest();

            var message = new Message
            {
                FromId = base.UserId,
                ToId = MessageModel.ToUserId,
                Content = MessageModel.Message,
                SentOn = DateTime.Now
            };
            await _chatContext.Messages.AddAsync(message, cancellationToken);
            if(await _chatContext.SaveChangesAsync(cancellationToken) > 0)
            {
                var responseMessageModel = new MessageModel(message.ToId, message.FromId, message.Content, message.SentOn);
                await _hubContext.Clients.User(MessageModel.ToUserId.ToString())
                            .MessageRecieved(responseMessageModel);

                await SendNotificationAsync(MessageModel);

                return Ok();
            }
            else
            {
                return StatusCode(500, "Unable to send message");
            }
        }

        
        [HttpGet("{otherUserId:int}")]
        public async Task<IEnumerable<MessageModel>> GetMessages(int otherUserId, CancellationToken cancellationToken)
        {
            var messages = await _chatContext.Messages
                            .AsNoTracking()
                            .Where(m =>
                                (m.FromId == otherUserId && m.ToId == UserId)
                                || (m.ToId == otherUserId && m.FromId == UserId)
                            )
                            .OrderBy(m => m.SentOn)
                            .Select(m=> new MessageModel(m.ToId, m.FromId, m.Content, m.SentOn))
                            .ToListAsync(cancellationToken);

            return messages;
        }




        private async Task SendNotificationAsync(MessageSendModel MessageModel)
        {
            // For a real application, generate your own
            var publicKey = "BKMWzJ1LyoFwXLU4bzvac-SDyaVsykCjmCOArq9LVumPaumaVEa5UrpDVA_KXL331BkPD5WizxdaJCLKXRDTxKg";
            var privateKey = "5-YQx7EkZyvOpu9hl7kQO-FsoUuP4VmdZZmiUxEorVU";

            var subscription = await _chatContext.NotificationSubscriptions.Where(e => e.UserId == MessageModel.ToUserId).SingleOrDefaultAsync();

            var pushSubscription = new PushSubscription(subscription?.Url, subscription?.P256dh, subscription?.Auth);
            var vapidDetails = new VapidDetails("mailto:admin@messenjoor.com", publicKey, privateKey);
            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    MessageModel.Message,
                    url = $"Messages/{MessageModel.ToUserId}",
                });
                await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error sending push notification: " + ex.Message);
            }
        }
    }
}
