namespace SmartDoc.SemanticKernel.Core.Qdrant.Models;

public class Usage
{
    public int Cpu { get; set; }
    public int PayloadIoRead { get; set; }
    public int PayloadIoWrite { get; set; }
    public int PayloadIndexIoRead { get; set; }
    public int PayloadIndexIoWrite { get; set; }
    public int VectorIoRead { get; set; }
    public int VectorIoWrite { get; set; }
}