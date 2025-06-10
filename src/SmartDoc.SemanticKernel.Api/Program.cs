#region using

using System;
using System.Net.Http;
using SmartDoc.SemanticKernel.Core.OpenAi.Interfaces;
using SmartDoc.SemanticKernel.Ingestion.OpenAi;
using SmartDoc.SemanticKernel.Ingestion.Services;
using SmartDoc.SemanticKernel.Memory.Qdrant;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using SmartDoc.SemanticKernel.Core.Interfaces;

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

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

//// Mock EmbeddingService (na razie)
//builder.Services.AddSingleton<IEmbeddingService, MockEmbeddingService>();

//builder.Services.AddSingleton<IOpenAiEmbeddingService, OpenAiEmbeddingService>();

builder.Services.AddSingleton<IEmbeddingService, OpenAiEmbeddingService>();

builder.Services.AddScoped<PdfChunkService>();

builder.Services.AddScoped<OpenAiChatService>();

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

builder.Services.AddHttpClient<OpenAiChatService>(client =>
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        client.BaseAddress = new Uri("https://api.openai.com");
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));


var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.Run();

//// Mock Embedding Service (implementacja)
//public class MockEmbeddingService : IEmbeddingService
//{
//    public Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
//    {
//        var rand = new Random();
//        return Task.FromResult(Enumerable.Range(0, 1536).Select(_ => (float)rand.NextDouble()).ToArray());
//    }
//}