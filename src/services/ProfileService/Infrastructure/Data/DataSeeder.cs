using Microsoft.EntityFrameworkCore;
using ProfileService.Domain.Entities;

namespace ProfileService.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ProfileDbContext db)
    {
        if (await db.CandidateProfiles.AnyAsync()) return;

        var candidates = new List<CandidateProfile>
        {
            new()
            {
                CandidateId = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                UserId = "c1000000-0000-0000-0000-000000000001",
                Phone = "+1-555-0101",
                Location = "San Francisco, CA",
                ProfessionalSummary = "Full-stack engineer with 6 years of experience building scalable web applications using React, Node.js, and C#. Passionate about clean architecture and developer tooling.",
                YearsOfExperience = 6,
                HighestEducation = "Bachelor's in Computer Science",
                LinkedinUrl = "https://linkedin.com/in/alicejohnson",
            },
            new()
            {
                CandidateId = Guid.Parse("a0000000-0000-0000-0000-000000000002"),
                UserId = "c1000000-0000-0000-0000-000000000002",
                Phone = "+1-555-0102",
                Location = "New York, NY",
                ProfessionalSummary = "Backend developer specializing in distributed systems and microservices. 4 years working with Java, Spring Boot, and Kubernetes in fintech.",
                YearsOfExperience = 4,
                HighestEducation = "Master's in Software Engineering",
                LinkedinUrl = "https://linkedin.com/in/bobwilliams",
            },
            new()
            {
                CandidateId = Guid.Parse("a0000000-0000-0000-0000-000000000003"),
                UserId = "c1000000-0000-0000-0000-000000000003",
                Phone = "+1-555-0103",
                Location = "Austin, TX",
                ProfessionalSummary = "DevOps engineer with 5 years of experience in cloud infrastructure, CI/CD pipelines, and container orchestration. AWS certified.",
                YearsOfExperience = 5,
                HighestEducation = "Bachelor's in Information Technology",
            },
            new()
            {
                CandidateId = Guid.Parse("a0000000-0000-0000-0000-000000000004"),
                UserId = "c1000000-0000-0000-0000-000000000004",
                Phone = "+1-555-0104",
                Location = "Seattle, WA",
                ProfessionalSummary = "Data engineer and Python specialist with 3 years of experience. Built data pipelines processing 10M+ records daily using Django, PostgreSQL, and Redis.",
                YearsOfExperience = 3,
                HighestEducation = "Bachelor's in Data Science",
            },
            new()
            {
                CandidateId = Guid.Parse("a0000000-0000-0000-0000-000000000005"),
                UserId = "c1000000-0000-0000-0000-000000000005",
                Phone = "+1-555-0105",
                Location = "Chicago, IL",
                ProfessionalSummary = "Frontend developer with 2 years of experience building responsive web apps with React, TypeScript, and Next.js. Strong eye for design.",
                YearsOfExperience = 2,
                HighestEducation = "Bachelor's in Computer Science",
            },
            new()
            {
                CandidateId = Guid.Parse("a0000000-0000-0000-0000-000000000006"),
                UserId = "c1000000-0000-0000-0000-000000000006",
                Phone = "+1-555-0106",
                Location = "Denver, CO",
                ProfessionalSummary = "Senior software engineer with 8 years of experience. Expert in C#, ASP.NET Core, and Azure. Led teams of 5-10 developers on enterprise applications.",
                YearsOfExperience = 8,
                HighestEducation = "Master's in Computer Science",
                LinkedinUrl = "https://linkedin.com/in/franknguyen",
            },
            new()
            {
                CandidateId = Guid.Parse("a0000000-0000-0000-0000-000000000007"),
                UserId = "c1000000-0000-0000-0000-000000000007",
                Phone = "+1-555-0107",
                Location = "Boston, MA",
                ProfessionalSummary = "Full-stack developer with 1 year of professional experience. Recent bootcamp graduate skilled in JavaScript, React, Node.js, and MongoDB.",
                YearsOfExperience = 1,
                HighestEducation = "Coding Bootcamp Certificate",
            },
            new()
            {
                CandidateId = Guid.Parse("a0000000-0000-0000-0000-000000000008"),
                UserId = "c1000000-0000-0000-0000-000000000008",
                Phone = "+1-555-0108",
                Location = "Portland, OR",
                ProfessionalSummary = "Cloud architect with 7 years of experience designing and implementing large-scale AWS and Terraform solutions. Strong background in Go and Rust.",
                YearsOfExperience = 7,
                HighestEducation = "Master's in Cloud Computing",
            },
        };

        var employers = new List<EmployerProfile>
        {
            new()
            {
                EmployerId = Guid.Parse("b0000000-0000-0000-0000-000000000001"),
                UserId = "e1000000-0000-0000-0000-000000000001",
                CompanyName = "TechCorp Solutions",
                CompanyDescription = "Enterprise software company building next-generation productivity tools. 500+ employees across 3 offices.",
                Industry = "Enterprise Software",
                WebsiteUrl = "https://techcorp.example.com",
                CompanyLocation = "San Francisco, CA",
            },
            new()
            {
                EmployerId = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
                UserId = "e1000000-0000-0000-0000-000000000002",
                CompanyName = "DataFlow Analytics",
                CompanyDescription = "Series B startup revolutionizing real-time data analytics. 80 employees, remote-first culture.",
                Industry = "Data Analytics",
                WebsiteUrl = "https://dataflow.example.io",
                CompanyLocation = "New York, NY",
            },
            new()
            {
                EmployerId = Guid.Parse("b0000000-0000-0000-0000-000000000003"),
                UserId = "e1000000-0000-0000-0000-000000000003",
                CompanyName = "CloudNine Infrastructure",
                CompanyDescription = "Cloud infrastructure provider specializing in managed Kubernetes and serverless platforms. 200+ employees.",
                Industry = "Cloud Computing",
                WebsiteUrl = "https://cloudnine.example.dev",
                CompanyLocation = "Seattle, WA",
            },
        };

        db.CandidateProfiles.AddRange(candidates);
        db.EmployerProfiles.AddRange(employers);
        await db.SaveChangesAsync();
    }
}
