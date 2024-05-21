using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Messenjoor.Components;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

Log.Logger = new LoggerConfiguration()
                        //.ReadFrom.Configuration(builder.Configuration)
                        .MinimumLevel.Is(Serilog.Events.LogEventLevel.Fatal)
                        .WriteTo.SQLite(sqliteDbPath: Environment.CurrentDirectory + @"/log.db")
                        .CreateLogger();


//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

//}).AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = TokenService.GetTokenValidationParameters(builder.Configuration);

//    options.Events = new JwtBearerEvents
//    {
//        OnMessageReceived = (context) =>
//        {
//            if (context.Request.Path.StartsWithSegments("/hubs/messenjoor"))
//            {
//                var jwt = context.Request.Query["access_token"];
//                if (!string.IsNullOrWhiteSpace(jwt))
//                {
//                    context.Token = jwt;
//                }
//            }
//            return Task.CompletedTask;
//        }
//    };
//});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwtBearerOptions =>
{
    jwtBearerOptions.RequireHttpsMetadata = true;
    jwtBearerOptions.SaveToken = true;
    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
    jwtBearerOptions.Events = new JwtBearerEvents
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

builder.Host.UseSerilog();

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

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatContext>();
    if (db.Database.EnsureCreated())
    {
        //SeedData.Initialize(db);
    }
}
//app.UseResponseCompression();


app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();


app.UseRouting();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapNotificationApi();
//app.MapRazorPages();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Messenjoor.UI._Imports).Assembly);
app.MapHub<MessenjoorHub>("/hubs/messenjoor");
Log.Information("Messenjoor Started...");
//app.MapFallbackToFile("index.html");
app.Run();



