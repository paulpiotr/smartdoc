#region using

using DevExpress.AI.Samples.Blazor.Components;
using DevExpress.Blazor;
using Microsoft.Extensions.AI;
using OpenAI;

#endregion

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{Environments.Development}.json", true, true)
    .AddJsonFile($"appsettings.{Environments.Staging}.json", true, true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory
});

builder.Configuration.AddConfiguration(configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var openAiKey = builder.Configuration["OpenAI:ApiKey"];
var client = new OpenAIClient(openAiKey ?? throw new NullReferenceException(nameof(openAiKey)));
builder.Services.AddDevExpressBlazor();
builder.Services.AddChatClient(client.AsChatClient("gpt-3.5-turbo"));
builder.Services.AddDevExpressAI();

builder.Services.AddDevExpressBlazor(options =>
{
    options.BootstrapVersion = BootstrapVersion.v5;
    options.SizeMode = SizeMode.Medium;
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();