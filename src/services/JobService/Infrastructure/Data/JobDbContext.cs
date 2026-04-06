using JobService.Domain.Entities;
using JobService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobService.Infrastructure.Data;

public class JobDbContext : DbContext
{
    public JobDbContext(DbContextOptions<JobDbContext> options) : base(options) { }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.JobId);
            entity.Property(e => e.JobId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EmployerId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => e.EmployerId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.EmploymentType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.JobStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.SalaryMin).HasPrecision(18, 2);
            entity.Property(e => e.SalaryMax).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.JobStatus, e.PostedAt });
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId);
            entity.Property(e => e.SkillId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SkillName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.SkillName).IsUnique();
            entity.Property(e => e.SkillCategory).HasMaxLength(50);
        });

        modelBuilder.Entity<JobSkill>(entity =>
        {
            entity.HasKey(e => e.JobSkillId);
            entity.Property(e => e.JobSkillId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ImportanceLevel).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(e => new { e.JobId, e.SkillId }).IsUnique();

            entity.HasOne(e => e.Job)
                .WithMany(j => j.JobSkills)
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Skill)
                .WithMany(s => s.JobSkills)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
