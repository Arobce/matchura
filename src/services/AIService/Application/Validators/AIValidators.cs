using AIService.Application.DTOs;
using FluentValidation;

namespace AIService.Application.Validators;

public class ComputeMatchRequestValidator : AbstractValidator<ComputeMatchRequest>
{
    public ComputeMatchRequestValidator()
    {
        RuleFor(x => x.JobId).NotEmpty().WithMessage("JobId is required");
    }
}

public class AnalyzeSkillGapRequestValidator : AbstractValidator<AnalyzeSkillGapRequest>
{
    public AnalyzeSkillGapRequestValidator()
    {
        RuleFor(x => x.JobId).NotEmpty().WithMessage("JobId is required");
    }
}
