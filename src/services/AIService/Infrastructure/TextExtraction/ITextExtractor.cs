namespace AIService.Infrastructure.TextExtraction;

public interface ITextExtractor
{
    bool CanHandle(string contentType);
    Task<string> ExtractTextAsync(Stream fileStream, CancellationToken ct = default);
}
