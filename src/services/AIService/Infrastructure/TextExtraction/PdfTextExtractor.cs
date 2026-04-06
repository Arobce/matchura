using System.Text;
using UglyToad.PdfPig;

namespace AIService.Infrastructure.TextExtraction;

public class PdfTextExtractor : ITextExtractor
{
    public bool CanHandle(string contentType) =>
        contentType == "application/pdf";

    public Task<string> ExtractTextAsync(Stream fileStream, CancellationToken ct = default)
    {
        using var document = PdfDocument.Open(fileStream);
        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return Task.FromResult(sb.ToString().Trim());
    }
}
