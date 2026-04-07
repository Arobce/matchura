using ApplicationService.Domain.Entities;
using ApplicationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ApplicationService.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Applications.AnyAsync()) return;

        var applications = new List<JobApplication>
        {
            // ── Alice (candidate 1) — applied to 4 jobs ──
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000001"),
                CandidateId = "c1000000-0000-0000-0000-000000000001",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000001"), // Senior Full-Stack
                CoverLetter = "I'm excited to apply for this role. With 6 years of full-stack experience in React and C#, I believe I'm a strong fit for the Senior Full-Stack Engineer position at TechCorp.",
                Status = ApplicationStatus.Shortlisted,
                EmployerNotes = "Strong candidate — 6 years full-stack, good culture fit.",
                AppliedAt = DateTime.UtcNow.AddDays(-12),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000002"),
                CandidateId = "c1000000-0000-0000-0000-000000000001",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000008"), // .NET Backend
                CoverLetter = "My background in C# and ASP.NET Core makes me well-suited for this backend developer role at CloudNine.",
                Status = ApplicationStatus.Reviewed,
                AppliedAt = DateTime.UtcNow.AddDays(-10),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000003"),
                CandidateId = "c1000000-0000-0000-0000-000000000001",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000006"), // React Frontend (DataFlow)
                CoverLetter = "I'd love to work on data visualization at DataFlow. I have strong React and TypeScript experience.",
                Status = ApplicationStatus.Submitted,
                AppliedAt = DateTime.UtcNow.AddDays(-2),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000004"),
                CandidateId = "c1000000-0000-0000-0000-000000000001",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000011"), // Closed: Mobile Dev
                CoverLetter = "I'd like to apply for the React Native position.",
                Status = ApplicationStatus.Rejected,
                EmployerNotes = "Position filled before review.",
                AppliedAt = DateTime.UtcNow.AddDays(-55),
            },

            // ── Bob (candidate 2) — applied to 3 jobs ──
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000005"),
                CandidateId = "c1000000-0000-0000-0000-000000000002",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000005"), // Backend (Java)
                CoverLetter = "With 4 years of Java and Spring Boot experience in fintech, I'm confident I can contribute to your analytics platform.",
                Status = ApplicationStatus.Accepted,
                EmployerNotes = "Offer extended. Start date: next month.",
                AppliedAt = DateTime.UtcNow.AddDays(-30),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000006"),
                CandidateId = "c1000000-0000-0000-0000-000000000002",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000004"), // Python Data Engineer
                CoverLetter = "While my primary language is Java, I have solid Python skills and interest in data engineering.",
                Status = ApplicationStatus.Reviewed,
                EmployerNotes = "Decent Java background but lacks Python depth.",
                AppliedAt = DateTime.UtcNow.AddDays(-8),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000007"),
                CandidateId = "c1000000-0000-0000-0000-000000000002",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000001"), // Senior Full-Stack
                CoverLetter = "I'm interested in transitioning to a full-stack role. I bring strong backend expertise.",
                Status = ApplicationStatus.Submitted,
                AppliedAt = DateTime.UtcNow.AddDays(-5),
            },

            // ── Carol (candidate 3) — applied to 2 jobs ──
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000008"),
                CandidateId = "c1000000-0000-0000-0000-000000000003",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000003"), // DevOps
                CoverLetter = "DevOps is my passion. 5 years with AWS, Kubernetes, and Terraform — this role is exactly what I'm looking for.",
                Status = ApplicationStatus.Shortlisted,
                EmployerNotes = "Top candidate for DevOps role. Schedule final interview.",
                AppliedAt = DateTime.UtcNow.AddDays(-18),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000009"),
                CandidateId = "c1000000-0000-0000-0000-000000000003",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000007"), // Cloud Platform (Go)
                CoverLetter = "I'd love to work on cloud platform engineering at CloudNine. My DevOps background is a strong foundation.",
                Status = ApplicationStatus.Reviewed,
                AppliedAt = DateTime.UtcNow.AddDays(-6),
            },

            // ── David (candidate 4) — applied to 2 jobs ──
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000010"),
                CandidateId = "c1000000-0000-0000-0000-000000000004",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000004"), // Python Data Engineer
                CoverLetter = "My 3 years of data engineering experience with Python, Django, and PostgreSQL make me a natural fit for this role.",
                Status = ApplicationStatus.Shortlisted,
                EmployerNotes = "Strong Python skills. Good pipeline experience.",
                AppliedAt = DateTime.UtcNow.AddDays(-9),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000011"),
                CandidateId = "c1000000-0000-0000-0000-000000000004",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000012"), // Closed: Data Analyst
                CoverLetter = "I'm interested in the data analyst position.",
                Status = ApplicationStatus.Accepted,
                EmployerNotes = "Hired for data analyst role.",
                AppliedAt = DateTime.UtcNow.AddDays(-40),
            },

            // ── Emma (candidate 5) — applied to 2 jobs ──
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000012"),
                CandidateId = "c1000000-0000-0000-0000-000000000005",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000002"), // Junior Frontend
                CoverLetter = "As a frontend developer with 2 years of React and TypeScript experience, I'm eager to grow at TechCorp.",
                Status = ApplicationStatus.Shortlisted,
                EmployerNotes = "Good portfolio, enthusiastic candidate.",
                AppliedAt = DateTime.UtcNow.AddDays(-6),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000013"),
                CandidateId = "c1000000-0000-0000-0000-000000000005",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000006"), // React Frontend (DataFlow)
                CoverLetter = "I love building interactive UIs and data visualization is a new area I'd love to explore.",
                Status = ApplicationStatus.Submitted,
                AppliedAt = DateTime.UtcNow.AddDays(-1),
            },

            // ── Frank (candidate 6) — applied to 3 jobs ──
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000014"),
                CandidateId = "c1000000-0000-0000-0000-000000000006",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000001"), // Senior Full-Stack
                CoverLetter = "With 8 years of C# and ASP.NET Core experience and a track record leading teams, I'm ready for this senior role.",
                Status = ApplicationStatus.Shortlisted,
                EmployerNotes = "Very experienced. Strong leadership skills. Top 3 candidate.",
                AppliedAt = DateTime.UtcNow.AddDays(-13),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000015"),
                CandidateId = "c1000000-0000-0000-0000-000000000006",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000008"), // .NET Backend
                CoverLetter = "My extensive C# background and Azure experience align perfectly with CloudNine's tech stack.",
                Status = ApplicationStatus.Accepted,
                EmployerNotes = "Offer accepted. Excellent hire.",
                AppliedAt = DateTime.UtcNow.AddDays(-20),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000016"),
                CandidateId = "c1000000-0000-0000-0000-000000000006",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000003"), // DevOps
                CoverLetter = "I have Docker and Azure experience from my enterprise projects.",
                Status = ApplicationStatus.Rejected,
                EmployerNotes = "Good dev but lacks core DevOps depth (K8s, Terraform).",
                AppliedAt = DateTime.UtcNow.AddDays(-19),
            },

            // ── Grace (candidate 7) — applied to 2 jobs ──
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000017"),
                CandidateId = "c1000000-0000-0000-0000-000000000007",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000002"), // Junior Frontend
                CoverLetter = "I'm a recent bootcamp graduate eager to start my career in frontend development.",
                Status = ApplicationStatus.Reviewed,
                EmployerNotes = "Bootcamp grad, limited experience but good attitude.",
                AppliedAt = DateTime.UtcNow.AddDays(-4),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000018"),
                CandidateId = "c1000000-0000-0000-0000-000000000007",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000009"), // Intern
                CoverLetter = "I'm looking for an internship to build my skills in cloud infrastructure.",
                Status = ApplicationStatus.Submitted,
                AppliedAt = DateTime.UtcNow.AddDays(-1),
            },

            // ── Henry (candidate 8) — applied to 2 jobs ──
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000019"),
                CandidateId = "c1000000-0000-0000-0000-000000000008",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000007"), // Cloud Platform (Go)
                CoverLetter = "7 years of cloud architecture with Go and Rust — I'm very interested in building your K8s platform.",
                Status = ApplicationStatus.Shortlisted,
                EmployerNotes = "Exceptional candidate. Strong Go + K8s experience.",
                AppliedAt = DateTime.UtcNow.AddDays(-7),
            },
            new()
            {
                ApplicationId = Guid.Parse("f0000000-0000-0000-0000-000000000020"),
                CandidateId = "c1000000-0000-0000-0000-000000000008",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000003"), // DevOps (TechCorp)
                CoverLetter = "My AWS and Terraform expertise combined with Go makes me a strong DevOps candidate.",
                Status = ApplicationStatus.Reviewed,
                AppliedAt = DateTime.UtcNow.AddDays(-15),
            },
        };

        db.Applications.AddRange(applications);
        await db.SaveChangesAsync();
    }
}
