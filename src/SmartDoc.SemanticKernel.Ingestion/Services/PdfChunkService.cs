#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartDoc.SemanticKernel.Core.Interfaces;
using SmartDoc.SemanticKernel.Core.Models;
using SmartDoc.SemanticKernel.Memory.Qdrant;
using UglyToad.PdfPig;

#endregion

namespace SmartDoc.SemanticKernel.Ingestion.Services;

public class PdfChunkService(IEmbeddingService embeddingService, IQdrantMemoryService qdrant)
{
    public async Task ProcessPdfAndSaveAsync(Stream pdfStream, string fileName, CancellationToken cancellationToken = default)
    {
        var text = ExtractTextFromPdf(pdfStream);
        var chunks = ChunkText(text, 512);
        var chunkModels = new List<DocumentChunk>();

        for (var i = 0; i < chunks.Count; i++)
        {
            var embedding = await embeddingService.GetEmbeddingAsync(chunks[i], cancellationToken);
            chunkModels.Add(new DocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                Text = chunks[i],
                Embedding = embedding,
                DocumentId = fileName,
                ChunkIndex = i
            });
        }

        await qdrant.PutChunksAsync(chunkModels);
    }

    public string ExtractTextFromPdf(Stream stream)
    {
        using var pdf = PdfDocument.Open(stream);
        var sb = new StringBuilder();
        foreach (var page in pdf.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }

    public List<string> ChunkText(string text, int chunkSize)
    {
        var chunks = new List<string>();
        for (var i = 0; i < text.Length; i += chunkSize)
        {
            var length = Math.Min(chunkSize, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }

        return chunks;
    }
}