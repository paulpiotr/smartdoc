#region using

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using SmartDoc.SemanticKernel.Core.OpenAi.Interfaces;
using SmartDoc.SemanticKernel.Memory.Qdrant;

#endregion

namespace SmartDoc.SemanticKernel.Client.OpenAi;

public class OpenAiChatService(
    IOpenAiEmbeddingService embeddingService,
    IQdrantMemoryService qdrant,
    IConfiguration config,
    HttpClient httpClient) : IChatClient
{
    private readonly string _apiKey = config["OpenAI:ApiKey"] ?? throw new NullReferenceException("OpenAI:ApiKey");

    public async Task<string> AskAsync(string question)
    {
        var embedding = await embeddingService.GetEmbeddingAsync(question);

        var relevantChunks = await qdrant.SearchChunksAsync(embedding, 5);

        var contextText = string.Join("\n---\n", relevantChunks.Select(c => c.Text));

        var prompt = $"Odpowiedz na pytanie na podstawie poniższych danych:\n\n{contextText}\n\nPytanie: {question}";

        var request = new
        {
            model = "gpt-4.1-mini",
            messages = new[]
            {
                new { role = "system", content = "Jesteś pomocnym asystentem." },
                new { role = "user",   content = prompt }
            },
            temperature = 0.5
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        string? reply = null;

        try
        {
            var response = await httpClient.PostAsync("/v1/chat/completions", content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var root = JsonDocument.Parse(json).RootElement;

            reply = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return reply ?? "[Brak odpowiedzi]";
    }

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var question = new { Question = messages.Last().Text };

        var reply = await AskAsync(question.Question);

        return new ChatResponse([
            new ChatMessage(ChatRole.Assistant, reply)
        ]);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var reply = await GetResponseAsync(messages, options, cancellationToken);
        foreach (var msg in reply.Messages)
        {
            yield return new ChatResponseUpdate(msg.Role, msg.Contents);
        }
    }

    public object GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType == typeof(HttpClient) ? httpClient : null!;
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}