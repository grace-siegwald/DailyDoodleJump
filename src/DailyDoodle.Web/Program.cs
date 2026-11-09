using DailyDoodle.Core.Games;
using DailyDoodle.Web;
using DailyDoodle.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Which game is open; shared by Home and Library.
builder.Services.AddSingleton<GameSession>();

// Finished days and best heights, saved in the browser.
builder.Services.AddSingleton<ProgressStore>();

await builder.Build().RunAsync();
