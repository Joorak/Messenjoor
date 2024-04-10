using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Messenjoor.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = TokenService.GetTokenValidationParameters(builder.Configuration);

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = (context) =>
        {
            if (context.Request.Path.StartsWithSegments("/hubs/messenjoor"))
            {
                var jwt = context.Request.Query["access_token"];
                if (!string.IsNullOrWhiteSpace(jwt))
                {
                    context.Token = jwt;
                }
            }
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddDbContext<ChatContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Chat")));

builder.Services.AddTransient<TokenService>();



builder.Services.AddSignalR();

//    .AddNewtonsoftJson((options) =>
//{
//    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
//});
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    //app.UseDeveloperExceptionPage();
    //builder.WebHost.UseUrls(builder.Configuration["HostURL"]!.ToString());
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseResponseCompression();

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();


app.UseRouting();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();


//app.MapRazorPages();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Messenjoor.UI._Imports).Assembly);
app.MapHub<MessenjoorHub>("/hubs/messenjoor");

//app.MapFallbackToFile("index.html");
app.Run();



