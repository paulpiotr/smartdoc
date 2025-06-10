#region using

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SmartDoc.SemanticKernel.Core.OpenAi.Interfaces;
using SmartDoc.SemanticKernel.Memory.Qdrant;
using Microsoft.Extensions.Configuration;

#endregion

namespace SmartDoc.SemanticKernel.Ingestion.OpenAi;

public class OpenAiChatService(
    IOpenAiEmbeddingService embeddingService,
    IQdrantMemoryService qdrant,
    IConfiguration config,
    HttpClient httpClient) : IOpenAiService
{
    private readonly string _apiKey = config["OpenAI:ApiKey"];

    public async Task<string> AskAsync(string question)
    {
        var embedding = await embeddingService.GetEmbeddingAsync(question);

        var relevantChunks = await qdrant.SearchChunksAsync(embedding, 5);

        var contextText = string.Join("\n---\n", relevantChunks.Select(c => c.Text));

        var prompt = $"Odpowiedz na pytanie na podstawie poniższych danych:\n\n{contextText}\n\nPytanie: {question}";

        var request = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "Jesteś asystentem." },
                new { role = "user",   content = prompt }
            },
            temperature = 0.2
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await httpClient.PostAsync("/v1/chat/completions", content);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var root = JsonDocument.Parse(json).RootElement;

        var reply = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        return reply ?? "[Brak odpowiedzi]";
    }
}