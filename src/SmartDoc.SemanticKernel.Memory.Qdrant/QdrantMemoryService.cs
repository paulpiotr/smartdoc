#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SmartDoc.SemanticKernel.Core.Models;
using SmartDoc.SemanticKernel.Core.Qdrant.Interfaces;
using SmartDoc.SemanticKernel.Core.Qdrant.Models;

#endregion

namespace SmartDoc.SemanticKernel.Memory.Qdrant;

public class QdrantMemoryService(HttpClient httpClient) : IQdrantMemoryService, IQdrantService
{
    public async Task PutChunksAsync(List<DocumentChunk> chunks)
    {
        await EnsureChunksCollectionExistsAsync();

        var batch = new
        {
            ids = chunks.Select(c => c.Id).ToArray(),
            vectors = chunks.Select(c => c.Embedding).ToArray(),
            payloads = chunks.Select(c => new { text = c.Text, documentId = c.DocumentId, chunkIndex = c.ChunkIndex })
                .ToArray()
        };

        var body = new { batch };

        var json = JsonSerializer.Serialize(body);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PutAsync("/collections/chunks/points?wait=true", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(error);
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<DocumentChunk>> SearchChunksAsync(float[] embedding, int topK)
    {
        var request = new
        {
            vector = embedding,
            top = topK,
            with_payload = true
        };

        var json = JsonSerializer.Serialize(request);

        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("/collections/chunks/points/search", stringContent);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var root = JsonDocument.Parse(content).RootElement;

        var results = new List<DocumentChunk>();

        foreach (var item in root.GetProperty("result").EnumerateArray())
        {
            var payload = item.GetProperty("payload");
            var chunk = new DocumentChunk
            {
                Id = item.GetProperty("id").ToString(),
                Text = payload.GetProperty("text").GetString() ?? string.Empty,
                DocumentId = payload.GetProperty("documentId").GetString() ?? string.Empty,
                ChunkIndex = payload.GetProperty("chunkIndex").GetInt32(),
                Embedding = embedding
            };

            results.Add(chunk);
        }

        return results;
    }

    public async Task EnsureChunksCollectionExistsAsync()
    {
        var response = await httpClient.GetAsync("/collections/chunks/exists");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var qdrantExistsResponse = JsonSerializer.Deserialize<QdrantExistsResponse>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            if (qdrantExistsResponse.Result.Exists)
            {
                return;
            }
        }

        var body = new
        {
            vectors = new
            {
                size = 1536,
                distance = "Cosine"
            }
        };

        var json = JsonSerializer.Serialize(body);

        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        response = await httpClient.PutAsync("/collections/chunks", stringContent);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(error);
        }

        response.EnsureSuccessStatusCode();
    }
}