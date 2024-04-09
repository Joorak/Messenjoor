using Messenjoor.Shared;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Messenjoor.WebApp.Controllers
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
    }
}
