using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TCGCollect;
using TCGCollect.DataStore;
using TCGCollect.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure CardStoreConfiguration using IConfiguration  
CardStoreConfiguration cardStoreConfiguration = new();
builder.Configuration.GetSection("CardStore").Bind(cardStoreConfiguration);
builder.Services.AddSingleton(cardStoreConfiguration);
builder.Services.AddSingleton<InMemoryCardStore>();
builder.Services.AddScoped<ICardService, CardService>();

var host = builder.Build();

// Initialize the InMemoryCardStore singleton
var cardStore = host.Services.GetRequiredService<InMemoryCardStore>();
await cardStore.Seed();

await host.RunAsync();
