namespace SmartDoc.SemanticKernel.Core.Qdrant.Models;

public class QdrantOperationResponse
{
    public Usage Usage { get; set; }
    public double Time { get; set; }
    public string Status { get; set; }
    public bool Result { get; set; }
}