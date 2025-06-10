#region using

using System.Collections.Generic;
using System.Threading.Tasks;
using SmartDoc.SemanticKernel.Core.Models;

#endregion

namespace SmartDoc.SemanticKernel.Memory.Qdrant;

public interface IQdrantMemoryService
{
    Task PutChunksAsync(List<DocumentChunk> chunks);

    Task<List<DocumentChunk>> SearchChunksAsync(float[] embedding, int topK);
}