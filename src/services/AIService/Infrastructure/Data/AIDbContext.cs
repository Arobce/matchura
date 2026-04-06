using AIService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIService.Infrastructure.Data;

public class AIDbContext : DbContext
{
    public AIDbContext(DbContextOptions<AIDbContext> options) : base(options) { }

    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<MatchScore> MatchScores => Set<MatchScore>();
    public DbSet<SkillGapReport> SkillGapReports => Set<SkillGapReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Resume>(entity =>
        {
            entity.HasKey(e => e.ResumeId);
            entity.Property(e => e.ResumeId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => e.CandidateId);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ParseStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.ParsedData).HasColumnType("jsonb");
        });

        modelBuilder.Entity<CandidateSkill>(entity =>
        {
            entity.HasKey(e => e.CandidateSkillId);
            entity.Property(e => e.CandidateSkillId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => e.CandidateId);
            entity.Property(e => e.SkillName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SkillCategory).HasMaxLength(50);
            entity.Property(e => e.ProficiencyLevel).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Source).HasMaxLength(20);
        });

        modelBuilder.Entity<MatchScore>(entity =>
        {
            entity.HasKey(e => e.MatchScoreId);
            entity.Property(e => e.MatchScoreId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => new { e.CandidateId, e.JobId }).IsUnique();
            entity.HasIndex(e => e.JobId);
            entity.Property(e => e.OverallScore).HasPrecision(5, 2);
            entity.Property(e => e.SkillScore).HasPrecision(5, 2);
            entity.Property(e => e.ExperienceScore).HasPrecision(5, 2);
            entity.Property(e => e.EducationScore).HasPrecision(5, 2);
            entity.Property(e => e.Explanation).HasMaxLength(2000);
            entity.Property(e => e.Strengths).HasColumnType("jsonb");
            entity.Property(e => e.Gaps).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SkillGapReport>(entity =>
        {
            entity.HasKey(e => e.ReportId);
            entity.Property(e => e.ReportId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => new { e.CandidateId, e.JobId }).IsUnique();
            entity.HasIndex(e => e.JobId);
            entity.Property(e => e.Summary).HasMaxLength(2000);
            entity.Property(e => e.OverallReadiness).HasPrecision(5, 2);
            entity.Property(e => e.EstimatedTimeToReady).HasMaxLength(50);
            entity.Property(e => e.MissingSkills).HasColumnType("jsonb");
            entity.Property(e => e.RecommendedActions).HasColumnType("jsonb");
            entity.Property(e => e.StrengthAreas).HasColumnType("jsonb");
        });
    }
}
