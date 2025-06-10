namespace SmartDoc.SemanticKernel.Core.Models;

public class DocumentChunk
{
    public string Id { get; set; }

    public string Text { get; set; }

    public float[] Embedding { get; set; }

    public string DocumentId { get; set; }

    public int ChunkIndex { get; set; }
}