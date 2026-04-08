using ApplicationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApplicationService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<JobApplication> Applications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId);
            entity.Property(e => e.ApplicationId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.CoverLetter).HasMaxLength(3000);
            entity.Property(e => e.CoverLetterUrl).HasMaxLength(500);
            entity.Property(e => e.ResumeUrl).HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.EmployerNotes).HasMaxLength(2000);

            // Prevent duplicate applications
            entity.HasIndex(e => new { e.CandidateId, e.JobId }).IsUnique();
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.CandidateId);
        });
    }
}
