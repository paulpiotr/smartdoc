#region using

using DevExpress.Blazor;
using Microsoft.Extensions.AI;
using Polly;
using Polly.Extensions.Http;
using SmartDoc.SemanticKernel.Client.OpenAi;
using SmartDoc.SemanticKernel.Core.OpenAi.Interfaces;
using SmartDoc.SemanticKernel.Ingestion.OpenAi;
using SmartDoc.SemanticKernel.Memory.Qdrant;
using SmartDoc.SemanticKernel.Ui.DxBlazorApplication.Components;

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

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<IQdrantMemoryService, QdrantMemoryService>(client =>
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        client.BaseAddress = new Uri("http://localhost:6333");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseDefaultCredentials = true,
        PreAuthenticate = true
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

builder.Services.AddHttpClient<IOpenAiEmbeddingService, OpenAiEmbeddingService>(client =>
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        client.BaseAddress = new Uri("https://api.openai.com");
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

builder.Services.AddHttpClient<IChatClient, OpenAiChatService>(client =>
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        client.BaseAddress = new Uri("https://api.openai.com");
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

//var openAiKey = builder.Configuration["OpenAI:ApiKey"];
//var client = new OpenAIClient(openAiKey ?? throw new NullReferenceException(nameof(openAiKey)));
//builder.Services.AddDevExpressBlazor();
//builder.Services.AddChatClient(client.AsChatClient("gpt-3.5-turbo"));

//builder.Services.AddScoped<IChatClient, OpenAiChatService>();

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