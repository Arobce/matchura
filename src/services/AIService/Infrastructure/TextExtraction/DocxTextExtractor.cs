using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AIService.Infrastructure.TextExtraction;

public class DocxTextExtractor : ITextExtractor
{
    public bool CanHandle(string contentType) =>
        contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public Task<string> ExtractTextAsync(Stream fileStream, CancellationToken ct = default)
    {
        using var doc = WordprocessingDocument.Open(fileStream, false);
        var body = doc.MainDocumentPart?.Document?.Body;

        if (body == null)
            return Task.FromResult(string.Empty);

        var sb = new StringBuilder();
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            sb.AppendLine(paragraph.InnerText);
        }

        return Task.FromResult(sb.ToString().Trim());
    }
}
