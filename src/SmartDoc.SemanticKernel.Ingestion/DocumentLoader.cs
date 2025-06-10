#region using

using System;
using System.IO;
using UglyToad.PdfPig;

#endregion

namespace SmartDoc.SemanticKernel.Ingestion;

public static class DocumentLoader
{
    public static string LoadAsText(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".txt" => File.ReadAllText(filePath),
            ".pdf" => LoadPdf(filePath),
            _ => throw new NotSupportedException($"Unsupported format: {ext}")
        };
    }

    private static string LoadPdf(string filePath)
    {
        using var doc = PdfDocument.Open(filePath);
        var writer = new StringWriter();
        foreach (var page in doc.GetPages())
            writer.WriteLine(page.Text);
        return writer.ToString();
    }
}