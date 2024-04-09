using BlazorPro.BlazorSize;
using Messenjoor.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMediaQueryService();
builder.Services.AddResizeListener(options =>
{
    options.ReportRate = 300;
    options.EnableLogging = false;
    options.SuppressInitEvent = false;
});


await builder.Build().RunAsync();
