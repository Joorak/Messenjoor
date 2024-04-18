using Messenjoor.Shared;
using Messenjoor.Shared.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Messenjoor.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ChatContext _chatContext;
        private readonly TokenService _tokenService;
        private readonly IHubContext<MessenjoorHub, IMessenjoorHubClient> _hubContext;
        

        public AccountController(ILogger<AccountController> logger, ChatContext chatContext, TokenService tokenService, IHubContext<MessenjoorHub, IMessenjoorHubClient> hubContext)
        {
            _chatContext = chatContext;
            _tokenService = tokenService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel dto, CancellationToken cancellationToken)
        {
            var usernameExists = await _chatContext.Users
                                        .AsNoTracking()
                                        .AnyAsync(u => u.Username == dto.Username, cancellationToken);

            if (usernameExists)
            {
                return BadRequest($"[{nameof(dto.Username)}] already exists");
            }

            var user = new User
            {
                Username = dto.Username,
                AddedOn = DateTime.Now,
                Name = dto.Name,
                Password = dto.Password, // Plain Password.  Implement your own secure password mechanism
            };

            await _chatContext.Users.AddAsync(user, cancellationToken);
            await _chatContext.SaveChangesAsync(cancellationToken);

            await _hubContext.Clients.All.UserConnected(new UserModel(user.Id, user.Name));

            return Ok(GenerateToken(user));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel dto, CancellationToken cancellationToken)
        {
            _logger.LogCritical($"login requested by : {JsonConverter.Serialize(dto)}");
            var user = await _chatContext.Users.FirstOrDefaultAsync(u=> u.Username == dto.Username && u.Password == dto.Password, cancellationToken);
            if(user is null)
            {
                _logger.LogCritical($"login failed for : {JsonConverter.Serialize(dto)}");
                return BadRequest("احراز هویت ناموفق...");
            }
            _logger.LogCritical($"successfull login for : {JsonConverter.Serialize(dto)}");
            return Ok(GenerateToken(user));
        }

        private AuthResponseModel GenerateToken(User user)
        {
            var token = _tokenService.GenerateJWT(user);
            return new AuthResponseModel(new UserModel(user.Id, user.Name), token);
        }
    }
}
