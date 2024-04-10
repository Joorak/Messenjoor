using Messenjoor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;



namespace Messenjoor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    
    public class MessenjoorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ChatContext _chatContext;
        private readonly TokenService _tokenService;
        public MessenjoorController(IConfiguration configuration, ChatContext chatContext, TokenService tokenService)
        {
            _configuration = configuration;
            _chatContext = chatContext;
            _tokenService = tokenService;
        }

        
        [HttpGet]
        public ContentResult Welcome()
        {

            //var html = System.IO.File.ReadAllText(@"./welcome.html");
            var html = $@"<!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='utf-8' />
                            <title>Welcome</title>
                        </head>
                        <body>
                            <h1>Welcome to Messenjoor</h1>
                            <p>Database Info : <strong>{_configuration["ConnectionStrings:Chat"]?.ToString()}</strong></p>
                        </body>
                        </html>";
            //html = html.Replace("{{name}}", name ?? "");

            return base.Content(html, "text/html");
        }

        [Authorize]
        
        [HttpGet("GetConnectionString")]
        public async Task<IActionResult> GetConnectionString()
        {
            string response = string.Empty;
            try
            {
                response = await Task.Run(() => _configuration["ConnectionStrings:Default"]!.ToString());
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("TestDb")]
        public async Task<IActionResult> TestInsertToDB()
        {
            var usernameExists = await _chatContext.Users
                                        .AsNoTracking()
                                        .AnyAsync(u => u.Username == "user99");

            if (usernameExists)
            {
                return BadRequest($"[user99] already exists");
            }

            var user = new User
            {
                Username = "کاربر تستی",
                AddedOn = DateTime.Now,
                Name = "user99",
                Password = "123", // Plain Password.  Implement your own secure password mechanism
            };

            await _chatContext.Users.AddAsync(user);
            await _chatContext.SaveChangesAsync();

            return Ok(_tokenService.GenerateJWT(user));
        }
    }
}