using Messenjoor.Shared;
using Messenjoor.Shared.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading;
using WebPush;

namespace Messenjoor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : BaseController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ChatContext _chatContext;
        private readonly IHubContext<MessenjoorHub, IMessenjoorHubClient> _hubContext;

        public MessagesController(ILogger<AccountController> logger, ChatContext chatContext, IHubContext<MessenjoorHub, IMessenjoorHubClient> hubContext)
        {
            _chatContext = chatContext;
            _hubContext = hubContext;
            _logger = logger;
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
                SentOn = DateTime.Now.ToUniversalTime()
            };

            await _chatContext.Messages.AddAsync(message, cancellationToken);
            
            if (await _chatContext.SaveChangesAsync(cancellationToken) > 0)
            {
                _logger.LogCritical($"{message.Content.PadRight(10).Substring(0, 10)}... from {message.FromId} sent to {message.ToId} saved in db");

                var responseMessageModel = new MessageModel(message.ToId, message.FromId, message.Content, message.SentOn);
                try
                {
                    await _hubContext.Clients.User(MessageModel.ToUserId.ToString())
                            .MessageRecieved(responseMessageModel);
                    _logger.LogCritical($"{message.Content.PadRight(10).Substring(0,10)}... from {message.FromId} sent to {message.ToId} published to hub MessageRecieved method");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical($"{message.Content.PadRight(10).Substring(0,10)}... from {message.FromId} sent to {message.ToId} publishing hub error {ex.Message}");
                }

                await SendNotificationAsync(message);



                return Ok();
            }
            else
            {
                _logger.LogCritical($"{message.Content.PadRight(10).Substring(0,10)}... from {message.FromId} sent to {message.ToId} db saving error");
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




        private async Task SendNotificationAsync(Message message)
        {
            // For a real application, generate your own
            var publicKey = "BKMWzJ1LyoFwXLU4bzvac-SDyaVsykCjmCOArq9LVumPaumaVEa5UrpDVA_KXL331BkPD5WizxdaJCLKXRDTxKg";
            var privateKey = "5-YQx7EkZyvOpu9hl7kQO-FsoUuP4VmdZZmiUxEorVU";

            var subscriptionExists = await _chatContext.NotificationSubscriptions
                                                        .AsNoTracking()
                                                        .AnyAsync(e => e.UserId == message.ToId);

            if (!subscriptionExists)
                return;
            

            var subscription = await _chatContext.NotificationSubscriptions.Where(e => e.UserId == message.ToId).SingleOrDefaultAsync();

            var pushSubscription = new PushSubscription(subscription?.Url, subscription?.P256dh, subscription?.Auth);
            var vapidDetails = new VapidDetails("mailto:admin@messenjoor.com", publicKey, privateKey);
            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    message.Content,
                    url = $"chat",
                });
                await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                _logger.LogCritical($"{message.Content.PadRight(10).Substring(0, 10)}... from {message.FromId} sent to {message.ToId} notification pushed");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"{message.Content.PadRight(10).Substring(0,10)}... from {message.FromId} sent to {message.ToId} sending push notification error : {ex.Message}");
                //Console.Error.WriteLine("Error sending push notification: " + ex.Message);
            }
        }
    }
}
