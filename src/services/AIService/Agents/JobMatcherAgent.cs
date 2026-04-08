using AIService.Agents.Core;
using AIService.Application.DTOs;

namespace AIService.Agents;

public class JobMatcherAgent
{
    private readonly ClaudeApiClient _client;

    private const string SystemPrompt = """
        You are a job matching expert. Analyze the candidate profile against the
        job requirements and provide a detailed match assessment.

        Return ONLY valid JSON:
        {
          "overallScore": <0-100>,
          "skillScore": <0-100>,
          "experienceScore": <0-100>,
          "educationScore": <0-100>,
          "explanation": "<2-3 sentence summary of fit>",
          "strengths": ["<strength 1>", "<strength 2>"],
          "gaps": ["<gap 1>", "<gap 2>"]
        }

        Scoring guide:
        - 90-100: Exceptional match, exceeds requirements
        - 75-89: Strong match, meets most requirements
        - 60-74: Moderate match, some gaps but transferable skills
        - 40-59: Weak match, significant gaps
        - 0-39: Poor match, major misalignment

        Consider:
        - Exact skill matches AND related/transferable skills
        - Years of experience relative to requirements
        - Industry relevance
        - Seniority level alignment
        """;

    public JobMatcherAgent(ClaudeApiClient client)
    {
        _client = client;
    }

    public virtual async Task<MatchResult> ComputeMatchAsync(string candidateData, string jobData, CancellationToken ct = default)
    {
        var userMessage = $"""
            CANDIDATE PROFILE:
            {candidateData}

            JOB REQUIREMENTS:
            {jobData}

            Analyze how well this candidate matches the job requirements.
            """;

        return await _client.SendAndParseAsync<MatchResult>(SystemPrompt, userMessage, ct);
    }
}
