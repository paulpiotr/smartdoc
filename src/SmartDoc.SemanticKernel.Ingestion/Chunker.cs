#region using

using System;
using System.Collections.Generic;

#endregion

namespace SmartDoc.SemanticKernel.Ingestion;

public static class Chunker
{
    public static IEnumerable<string> Split(string text, int maxChunkSize = 1000)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        var sentences = text.Split([". "], StringSplitOptions.None);
        var chunk = "";
        foreach (var sentence in sentences)
        {
            if ((chunk + sentence).Length > maxChunkSize)
            {
                yield return chunk.Trim();
                chunk = "";
            }

            chunk += sentence + ". ";
        }

        if (!string.IsNullOrWhiteSpace(chunk))
            yield return chunk.Trim();
    }
}