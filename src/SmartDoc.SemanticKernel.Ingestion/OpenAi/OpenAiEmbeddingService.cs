#region using

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SmartDoc.SemanticKernel.Core.OpenAi.Interfaces;
using Microsoft.Extensions.Configuration;

#endregion

namespace SmartDoc.SemanticKernel.Ingestion.OpenAi;

public class OpenAiEmbeddingService(IConfiguration config, HttpClient httpClient)
    : IOpenAiEmbeddingService
{
    private readonly string _apiKey = config["OpenAI:ApiKey"];

    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            input = text,
            model = "text-embedding-ada-002",
            encoding_format = "float"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await httpClient.PostAsync("/v1/embeddings", content, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        using var jsonDoc = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

        var vector = jsonDoc.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();

        return vector;
    }
}