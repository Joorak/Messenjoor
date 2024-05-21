using Messenjoor.Shared;
using Messenjoor.Shared.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Messenjoor.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly ChatContext _chatContext;
        private readonly TokenService _tokenService;
        private readonly IHubContext<MessenjoorHub, IMessenjoorHubClient> _hubContext;
        

        public AccountController(IConfiguration configuration, ILogger<AccountController> logger, ChatContext chatContext, TokenService tokenService, IHubContext<MessenjoorHub, IMessenjoorHubClient> hubContext)
        {
            _configuration = configuration;
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

        [HttpGet("ValidateToken/{token}")]
        public async Task<IActionResult> ValidateToken(string token)
        {
            string secretKey = _configuration["Jwt:Key"] ?? "";
            var key = Encoding.ASCII.GetBytes(secretKey);

            //preparing the validation parameters
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            //SecurityToken securityToken;

            //validating the token
            var tokenValidationResult = await tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);
            var jwtSecurityToken = tokenValidationResult.SecurityToken;
            if (jwtSecurityToken != null
                && jwtSecurityToken.ValidTo > DateTime.Now)
            {
                return Ok(true);

            }
            else
            {
                return Ok(false);
            }
        }
    }
}
