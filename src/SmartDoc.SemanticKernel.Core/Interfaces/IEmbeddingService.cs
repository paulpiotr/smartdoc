#region using

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace SmartDoc.SemanticKernel.Core.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}