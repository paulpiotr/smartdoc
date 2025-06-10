#region using

#endregion

namespace SmartDoc.SemanticKernel.Ingestion;

public class QdrantIngestionService
{
    //private const string Namespace = "docs";
    //private readonly EmbeddingService _embeddingService;
    //private readonly ISemanticTextMemory _memory;

    //public QdrantIngestionService(EmbeddingService embeddingService, ISemanticTextMemory memory)
    //{
    //    _embeddingService = embeddingService;
    //    _memory = memory;
    //}

    //public async Task IngestAsync(string filePath)
    //{
    //    var text = DocumentLoader.LoadAsText(filePath);
    //    var chunks = Chunker.Split(text);
    //    foreach (var chunk in chunks)
    //    {
    //        var embedding = await _embeddingService.GenerateAsync(chunk);
    //        await _memory.SaveInformationAsync(Namespace, Guid.NewGuid().ToString(), chunk, embedding.ToArray());
    //    }
    //}
}