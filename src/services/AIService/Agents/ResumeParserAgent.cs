using AIService.Agents.Core;
using AIService.Application.DTOs;

namespace AIService.Agents;

public class ResumeParserAgent
{
    private readonly ClaudeApiClient _client;

    private const string SystemPrompt = """
        You are a resume parsing assistant. Extract structured information from
        the resume text provided. Return ONLY valid JSON matching the exact schema
        below. Do not include any text outside the JSON object.

        If a field cannot be determined from the resume, use null.
        For skills, infer proficiency level from context (years mentioned,
        how prominently featured, whether listed as "expert" or "familiar with").
        Categorize skills into: Programming, Framework, Database, DevOps, Cloud,
        Design, Soft Skills, Other.

        JSON Schema:
        {
          "personalInfo": { "name": string|null, "email": string|null, "phone": string|null, "location": string|null },
          "summary": string|null,
          "experience": [{ "company": string, "title": string, "startDate": string|null, "endDate": string|null, "description": string, "highlights": string[] }],
          "education": [{ "institution": string, "degree": string, "field": string, "graduationDate": string|null, "gpa": number|null }],
          "skills": [{ "name": string, "category": string, "proficiencyLevel": "Beginner"|"Intermediate"|"Advanced"|"Expert", "yearsUsed": number|null }],
          "certifications": [{ "name": string, "issuer": string|null, "date": string|null }],
          "projects": [{ "name": string, "description": string, "technologies": string[] }]
        }
        """;

    public ResumeParserAgent(ClaudeApiClient client)
    {
        _client = client;
    }

    public virtual async Task<ParsedResumeData> ParseAsync(string rawText, CancellationToken ct = default)
    {
        var userMessage = $"Parse the following resume and extract structured data:\n\n{rawText}";
        return await _client.SendAndParseAsync<ParsedResumeData>(SystemPrompt, userMessage, ct);
    }
}
