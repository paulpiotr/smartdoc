#region using

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Qdrant.Client;

#endregion

namespace SmartDoc.SemanticKernel.Core;

public static class KernelConfig
{
    [Experimental("SKEXP0010")]
    public static IServiceCollection AddSemanticKernelServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1) OpenAI: ChatCompletion
        services.AddOpenAIChatCompletion(
            configuration["OpenAI:Model"] ?? "gpt-4",
            configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException());

        // 2) OpenAI: EmbeddingGenerator (Zamiast AddOpenAITextEmbeddingGeneration)
        services.AddOpenAIEmbeddingGenerator(
            configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-ada-002",
            configuration["OpenAI:ApiKey"]);

        // 3) QdrantClient + QdrantVectorStore
        services.AddSingleton(sp =>
        {
            var host = configuration["Qdrant:Host"];
            var apiKey = configuration["Qdrant:ApiKey"];
            return new QdrantClient(host, apiKey: apiKey);
        });
        services.AddQdrantVectorStore();

        // 4) Rejestracja IKernel (metod¹ AddKernel)
        services.AddKernel();

        return services;
    }
}