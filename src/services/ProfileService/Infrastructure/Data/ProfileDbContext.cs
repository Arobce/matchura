using Microsoft.EntityFrameworkCore;
using ProfileService.Domain.Entities;

namespace ProfileService.Infrastructure.Data;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options) { }

    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CandidateProfile>(entity =>
        {
            entity.HasKey(e => e.CandidateId);
            entity.Property(e => e.CandidateId).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.ProfessionalSummary).HasMaxLength(2000);
            entity.Property(e => e.HighestEducation).HasMaxLength(200);
            entity.Property(e => e.LinkedinUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<EmployerProfile>(entity =>
        {
            entity.HasKey(e => e.EmployerId);
            entity.Property(e => e.EmployerId).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CompanyDescription).HasMaxLength(2000);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.WebsiteUrl).HasMaxLength(500);
            entity.Property(e => e.CompanyLocation).HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
        });
    }
}
