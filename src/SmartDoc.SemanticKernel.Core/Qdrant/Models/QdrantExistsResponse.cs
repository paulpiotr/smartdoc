namespace SmartDoc.SemanticKernel.Core.Qdrant.Models;

public class QdrantExistsResponse
{
    public double Time { get; set; }
    public string Status { get; set; }
    public ResultModel Result { get; set; }

    public class ResultModel
    {
        public bool Exists { get; set; }
    }
}