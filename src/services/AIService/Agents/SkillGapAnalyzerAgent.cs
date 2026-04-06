using AIService.Agents.Core;
using AIService.Application.DTOs;

namespace AIService.Agents;

public class SkillGapAnalyzerAgent
{
    private readonly ClaudeApiClient _client;

    private const string SystemPrompt = """
        You are a career development advisor. Analyze the gap between a candidate's
        current skills and a job's requirements. Provide actionable, specific advice.

        Return ONLY valid JSON:
        {
          "summary": "<2-3 sentence overview of readiness>",
          "overallReadiness": <0-100>,
          "estimatedTimeToReady": "<e.g. '2-4 months'>",
          "missingSkills": [
            {
              "skillName": "<skill>",
              "importance": "Critical" | "Important" | "NiceToHave",
              "currentLevel": "None" | "Beginner" | "Intermediate" | null,
              "requiredLevel": "Intermediate" | "Advanced" | "Expert",
              "gapSeverity": <1-10>,
              "recommendation": "<specific learning path>"
            }
          ],
          "recommendedActions": [
            {
              "priority": <1-N>,
              "action": "<specific actionable step>",
              "estimatedTime": "<e.g. '2 weeks'>",
              "resourceType": "Course" | "Project" | "Certification" | "Practice",
              "rationale": "<why this helps>"
            }
          ],
          "strengths": ["<areas where candidate exceeds requirements>"]
        }
        """;

    public SkillGapAnalyzerAgent(ClaudeApiClient client)
    {
        _client = client;
    }

    public async Task<SkillGapResult> AnalyzeAsync(string candidateData, string jobData, CancellationToken ct = default)
    {
        var userMessage = $"""
            CANDIDATE SKILLS AND PROFILE:
            {candidateData}

            JOB REQUIREMENTS:
            {jobData}

            Analyze the skill gaps and provide actionable recommendations.
            """;

        return await _client.SendAndParseAsync<SkillGapResult>(SystemPrompt, userMessage, ct);
    }
}
