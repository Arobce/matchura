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
                CompanyName = "Google",
                CompanyDescription = "Google builds products and technology that help people do more. From Search and Maps to Cloud and YouTube, Google's mission is to organize the world's information and make it universally accessible. With 180,000+ employees worldwide, Google is one of the most influential technology companies in the world.",
                Industry = "Technology",
                WebsiteUrl = "https://careers.google.com",
                CompanyLocation = "Mountain View, CA",
            },
            new()
            {
                EmployerId = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
                UserId = "e1000000-0000-0000-0000-000000000002",
                CompanyName = "Stripe",
                CompanyDescription = "Stripe is a financial infrastructure platform for the internet. Millions of companies — from the world's largest enterprises to the most ambitious startups — use Stripe to accept payments, grow their revenue, and accelerate new business opportunities. Headquartered in San Francisco with 8,000+ employees globally.",
                Industry = "Financial Technology",
                WebsiteUrl = "https://stripe.com/jobs",
                CompanyLocation = "San Francisco, CA",
            },
            new()
            {
                EmployerId = Guid.Parse("b0000000-0000-0000-0000-000000000003"),
                UserId = "e1000000-0000-0000-0000-000000000003",
                CompanyName = "Amazon",
                CompanyDescription = "Amazon is guided by four principles: customer obsession, passion for invention, commitment to operational excellence, and long-term thinking. From e-commerce to cloud computing (AWS) to entertainment and devices, Amazon is one of the most customer-centric companies on Earth with 1.5 million+ employees.",
                Industry = "Technology & E-Commerce",
                WebsiteUrl = "https://amazon.jobs",
                CompanyLocation = "Seattle, WA",
            },
            new()
            {
                EmployerId = Guid.Parse("b0000000-0000-0000-0000-000000000004"),
                UserId = "e1000000-0000-0000-0000-000000000004",
                CompanyName = "Meta",
                CompanyDescription = "Meta builds technologies that help people connect, find communities, and grow businesses. From its family of apps — Facebook, Instagram, WhatsApp, and Messenger — to Reality Labs and its work on the metaverse, Meta is reshaping how people interact online. 70,000+ employees across 80+ offices worldwide.",
                Industry = "Social Technology",
                WebsiteUrl = "https://metacareers.com",
                CompanyLocation = "Menlo Park, CA",
            },
            new()
            {
                EmployerId = Guid.Parse("b0000000-0000-0000-0000-000000000005"),
                UserId = "e1000000-0000-0000-0000-000000000005",
                CompanyName = "Microsoft",
                CompanyDescription = "Microsoft's mission is to empower every person and every organization on the planet to achieve more. From Azure and Microsoft 365 to GitHub, LinkedIn, and Xbox, Microsoft builds platforms and tools that power billions of people's work and lives. 220,000+ employees in 100+ countries.",
                Industry = "Technology",
                WebsiteUrl = "https://careers.microsoft.com",
                CompanyLocation = "Redmond, WA",
            },
            new()
            {
                EmployerId = Guid.Parse("b0000000-0000-0000-0000-000000000006"),
                UserId = "e1000000-0000-0000-0000-000000000006",
                CompanyName = "Netflix",
                CompanyDescription = "Netflix is the world's leading streaming entertainment service with 260+ million paid memberships in over 190 countries. Netflix offers a wide variety of TV series, documentaries, feature films, and games across a wide variety of genres and languages. 13,000+ employees focused on creating joy through storytelling.",
                Industry = "Entertainment & Streaming",
                WebsiteUrl = "https://jobs.netflix.com",
                CompanyLocation = "Los Gatos, CA",
            },
        };

        db.CandidateProfiles.AddRange(candidates);
        db.EmployerProfiles.AddRange(employers);
        await db.SaveChangesAsync();
    }
}
