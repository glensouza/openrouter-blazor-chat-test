using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace OpenRouterChat.Services;

public class DocumentService
{
    private const int MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    private const int MaxContextChars = 50_000;

    public async Task<string> ExtractTextAsync(Stream stream, string fileName, long? knownSize = null)
    {
        long fileSize = knownSize ?? (stream.CanSeek ? stream.Length : -1);
        if (fileSize > MaxFileSizeBytes)
            throw new InvalidOperationException($"File exceeds the 10 MB limit.");

        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => await ExtractPdfTextAsync(stream),
            ".txt" or ".md" or ".csv" or ".json" or ".xml" or ".html" or ".htm" => await ExtractPlainTextAsync(stream),
            _ => throw new NotSupportedException($"File type '{extension}' is not supported. Supported types: .pdf, .txt, .md, .csv, .json, .xml, .html")
        };
    }

    private static async Task<string> ExtractPdfTextAsync(Stream stream)
    {
        // Read into memory to allow seeking (PdfPig requires seekable stream)
        using MemoryStream ms = new();
        await stream.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);

        using PdfDocument? document = PdfDocument.Open(ms);
        StringBuilder sb = new();
        foreach (Page? page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        string text = sb.ToString();
        if (text.Length > MaxContextChars)
            text = text[..MaxContextChars] + "\n\n[Content truncated due to length...]";
        return text;
    }

    private static async Task<string> ExtractPlainTextAsync(Stream stream)
    {
        using StreamReader reader = new(stream, leaveOpen: true);
        string text = await reader.ReadToEndAsync();
        if (text.Length > MaxContextChars)
            text = text[..MaxContextChars] + "\n\n[Content truncated due to length...]";
        return text;
    }
}
