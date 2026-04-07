using JobService.Domain.Entities;
using JobService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobService.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedSkillsAsync(JobDbContext db)
    {
        if (await db.Skills.AnyAsync()) return;

        var skills = new List<Skill>
        {
            // Programming Languages
            new() { SkillName = "C#", SkillCategory = "Programming" },
            new() { SkillName = "JavaScript", SkillCategory = "Programming" },
            new() { SkillName = "TypeScript", SkillCategory = "Programming" },
            new() { SkillName = "Python", SkillCategory = "Programming" },
            new() { SkillName = "Java", SkillCategory = "Programming" },
            new() { SkillName = "Go", SkillCategory = "Programming" },
            new() { SkillName = "Rust", SkillCategory = "Programming" },
            new() { SkillName = "SQL", SkillCategory = "Programming" },

            // Frameworks
            new() { SkillName = "ASP.NET Core", SkillCategory = "Frameworks" },
            new() { SkillName = "React", SkillCategory = "Frameworks" },
            new() { SkillName = "Next.js", SkillCategory = "Frameworks" },
            new() { SkillName = "Angular", SkillCategory = "Frameworks" },
            new() { SkillName = "Node.js", SkillCategory = "Frameworks" },
            new() { SkillName = "Spring Boot", SkillCategory = "Frameworks" },
            new() { SkillName = "Django", SkillCategory = "Frameworks" },
            new() { SkillName = "Express.js", SkillCategory = "Frameworks" },

            // DevOps
            new() { SkillName = "Docker", SkillCategory = "DevOps" },
            new() { SkillName = "Kubernetes", SkillCategory = "DevOps" },
            new() { SkillName = "AWS", SkillCategory = "DevOps" },
            new() { SkillName = "Azure", SkillCategory = "DevOps" },
            new() { SkillName = "CI/CD", SkillCategory = "DevOps" },
            new() { SkillName = "Terraform", SkillCategory = "DevOps" },

            // Databases
            new() { SkillName = "PostgreSQL", SkillCategory = "Databases" },
            new() { SkillName = "MongoDB", SkillCategory = "Databases" },
            new() { SkillName = "Redis", SkillCategory = "Databases" },
            new() { SkillName = "SQL Server", SkillCategory = "Databases" },

            // Soft Skills
            new() { SkillName = "Communication", SkillCategory = "Soft Skills" },
            new() { SkillName = "Leadership", SkillCategory = "Soft Skills" },
            new() { SkillName = "Problem Solving", SkillCategory = "Soft Skills" },
            new() { SkillName = "Teamwork", SkillCategory = "Soft Skills" },
        };

        db.Skills.AddRange(skills);
        await db.SaveChangesAsync();
    }

    public static async Task SeedJobsAsync(JobDbContext db)
    {
        if (await db.Jobs.AnyAsync()) return;

        // Load skill IDs by name
        var skills = await db.Skills.ToDictionaryAsync(s => s.SkillName, s => s.SkillId);

        var jobs = new List<Job>
        {
            // ── TechCorp Solutions (employer 1) ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000001"),
                EmployerId = "e1000000-0000-0000-0000-000000000001",
                Title = "Senior Full-Stack Engineer",
                Description = "We're looking for a senior full-stack engineer to lead development of our flagship SaaS product. You'll architect new features, mentor junior developers, and work closely with product to ship impactful features every sprint.\n\nResponsibilities:\n- Design and implement features end-to-end (React frontend + C# backend)\n- Lead code reviews and set technical standards\n- Collaborate with product managers on roadmap priorities\n- Optimize application performance and scalability",
                Location = "San Francisco, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 5,
                SalaryMin = 150000m,
                SalaryMax = 200000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-14),
                ApplicationDeadline = DateTime.UtcNow.AddDays(30),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["C#"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["React"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["TypeScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["PostgreSQL"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Leadership"], ImportanceLevel = ImportanceLevel.NiceToHave },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000002"),
                EmployerId = "e1000000-0000-0000-0000-000000000001",
                Title = "Junior Frontend Developer",
                Description = "Join our growing frontend team and help build beautiful, accessible UIs for our enterprise clients. Perfect for developers early in their career who want mentorship and growth.\n\nResponsibilities:\n- Build responsive UI components with React and TypeScript\n- Write unit and integration tests\n- Participate in design reviews with UX team",
                Location = "San Francisco, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 1,
                SalaryMin = 80000m,
                SalaryMax = 110000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-7),
                ApplicationDeadline = DateTime.UtcNow.AddDays(45),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["JavaScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["React"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["TypeScript"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Next.js"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["Teamwork"], ImportanceLevel = ImportanceLevel.Required },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000003"),
                EmployerId = "e1000000-0000-0000-0000-000000000001",
                Title = "DevOps Engineer",
                Description = "Own our CI/CD pipelines, cloud infrastructure, and deployment processes. We run on AWS with Kubernetes and are migrating to a fully Terraform-managed infrastructure.\n\nResponsibilities:\n- Maintain and improve CI/CD pipelines\n- Manage Kubernetes clusters and Docker deployments\n- Implement infrastructure-as-code with Terraform\n- Monitor system health and respond to incidents",
                Location = "San Francisco, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 3,
                SalaryMin = 130000m,
                SalaryMax = 170000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-21),
                ApplicationDeadline = DateTime.UtcNow.AddDays(14),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Kubernetes"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["AWS"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Terraform"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["CI/CD"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Python"], ImportanceLevel = ImportanceLevel.NiceToHave },
                },
            },

            // ── DataFlow Analytics (employer 2) ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000004"),
                EmployerId = "e1000000-0000-0000-0000-000000000002",
                Title = "Python Data Engineer",
                Description = "Build and maintain our real-time data pipelines processing millions of events per day. You'll work with Python, PostgreSQL, and Redis to deliver insights to our customers in under 100ms.\n\nResponsibilities:\n- Design and build ETL/ELT pipelines\n- Optimize database queries and data models\n- Build internal tools and APIs with Django\n- Monitor data quality and pipeline health",
                Location = "New York, NY",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 3,
                SalaryMin = 120000m,
                SalaryMax = 160000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-10),
                ApplicationDeadline = DateTime.UtcNow.AddDays(35),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Python"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["PostgreSQL"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Redis"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Django"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["SQL"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.NiceToHave },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000005"),
                EmployerId = "e1000000-0000-0000-0000-000000000002",
                Title = "Backend Engineer (Java)",
                Description = "Join our core platform team building the backbone of our analytics engine. We use Java with Spring Boot and handle high-throughput data processing at scale.\n\nResponsibilities:\n- Build RESTful APIs and microservices\n- Design event-driven architectures\n- Write comprehensive tests and documentation\n- Participate in on-call rotation",
                Location = "Remote",
                EmploymentType = EmploymentType.Remote,
                ExperienceRequired = 4,
                SalaryMin = 140000m,
                SalaryMax = 180000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-5),
                ApplicationDeadline = DateTime.UtcNow.AddDays(40),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Java"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Spring Boot"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["PostgreSQL"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Kubernetes"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["Communication"], ImportanceLevel = ImportanceLevel.Required },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000006"),
                EmployerId = "e1000000-0000-0000-0000-000000000002",
                Title = "React Frontend Developer",
                Description = "Help us build a beautiful, intuitive analytics dashboard that our customers love. We use React, TypeScript, and Next.js with a strong focus on data visualization.\n\nResponsibilities:\n- Build interactive data visualization components\n- Implement responsive designs from Figma specs\n- Optimize frontend performance for large datasets\n- Collaborate with backend engineers on API contracts",
                Location = "New York, NY",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 2,
                SalaryMin = 100000m,
                SalaryMax = 140000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-3),
                ApplicationDeadline = DateTime.UtcNow.AddDays(50),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["React"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["TypeScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Next.js"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["JavaScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Problem Solving"], ImportanceLevel = ImportanceLevel.Preferred },
                },
            },

            // ── CloudNine Infrastructure (employer 3) ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000007"),
                EmployerId = "e1000000-0000-0000-0000-000000000003",
                Title = "Cloud Platform Engineer (Go)",
                Description = "Build the next generation of our managed Kubernetes platform. We're looking for engineers who love Go, understand distributed systems, and want to work on core infrastructure.\n\nResponsibilities:\n- Develop control plane services in Go\n- Build Kubernetes operators and controllers\n- Design multi-tenant infrastructure components\n- Write performance benchmarks and load tests",
                Location = "Seattle, WA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 4,
                SalaryMin = 160000m,
                SalaryMax = 210000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-8),
                ApplicationDeadline = DateTime.UtcNow.AddDays(30),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Go"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Kubernetes"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["AWS"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Terraform"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Rust"], ImportanceLevel = ImportanceLevel.NiceToHave },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000008"),
                EmployerId = "e1000000-0000-0000-0000-000000000003",
                Title = ".NET Backend Developer",
                Description = "Join our billing and identity team building mission-critical services in C# and ASP.NET Core. You'll work on authentication, authorization, and payment processing.\n\nResponsibilities:\n- Build and maintain REST APIs with ASP.NET Core\n- Design and optimize SQL Server and PostgreSQL schemas\n- Implement security best practices for payment flows\n- Write integration tests and API documentation",
                Location = "Seattle, WA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 3,
                SalaryMin = 130000m,
                SalaryMax = 165000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-12),
                ApplicationDeadline = DateTime.UtcNow.AddDays(25),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["C#"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["ASP.NET Core"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["PostgreSQL"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["SQL Server"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Azure"], ImportanceLevel = ImportanceLevel.NiceToHave },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000009"),
                EmployerId = "e1000000-0000-0000-0000-000000000003",
                Title = "Infrastructure Intern",
                Description = "12-week paid internship working alongside senior infrastructure engineers. Learn cloud computing, Terraform, and Kubernetes in a hands-on environment.\n\nWhat you'll learn:\n- Cloud infrastructure fundamentals (AWS)\n- Infrastructure-as-code with Terraform\n- Container orchestration with Docker & Kubernetes\n- Monitoring and observability practices",
                Location = "Seattle, WA",
                EmploymentType = EmploymentType.Internship,
                ExperienceRequired = 0,
                SalaryMin = 35000m,
                SalaryMax = 45000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-2),
                ApplicationDeadline = DateTime.UtcNow.AddDays(60),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Python"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["AWS"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["Teamwork"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Problem Solving"], ImportanceLevel = ImportanceLevel.Required },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000010"),
                EmployerId = "e1000000-0000-0000-0000-000000000001",
                Title = "Part-Time QA Engineer",
                Description = "Looking for a detail-oriented QA engineer to join part-time (20 hrs/week). You'll build automated test suites and ensure product quality across our platform.\n\nResponsibilities:\n- Write and maintain automated test suites\n- Perform manual exploratory testing\n- Report and track bugs with clear reproduction steps\n- Collaborate with developers on test coverage goals",
                Location = "San Francisco, CA",
                EmploymentType = EmploymentType.PartTime,
                ExperienceRequired = 2,
                SalaryMin = 50000m,
                SalaryMax = 65000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-1),
                ApplicationDeadline = DateTime.UtcNow.AddDays(30),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["JavaScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Python"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["SQL"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Communication"], ImportanceLevel = ImportanceLevel.Required },
                },
            },

            // ── Closed/expired jobs for stats variety ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000011"),
                EmployerId = "e1000000-0000-0000-0000-000000000001",
                Title = "Mobile Developer (React Native)",
                Description = "We were looking for a React Native developer for our mobile app. This position has been filled.",
                Location = "San Francisco, CA",
                EmploymentType = EmploymentType.Contract,
                ExperienceRequired = 2,
                SalaryMin = 90000m,
                SalaryMax = 120000m,
                JobStatus = JobStatus.Closed,
                PostedAt = DateTime.UtcNow.AddDays(-60),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["JavaScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["React"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["TypeScript"], ImportanceLevel = ImportanceLevel.Preferred },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000012"),
                EmployerId = "e1000000-0000-0000-0000-000000000002",
                Title = "Data Analyst",
                Description = "Previously open data analyst position. This role has been filled.",
                Location = "New York, NY",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 1,
                SalaryMin = 70000m,
                SalaryMax = 95000m,
                JobStatus = JobStatus.Closed,
                PostedAt = DateTime.UtcNow.AddDays(-45),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Python"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["SQL"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["PostgreSQL"], ImportanceLevel = ImportanceLevel.Preferred },
                },
            },
        };

        db.Jobs.AddRange(jobs);
        await db.SaveChangesAsync();
    }
}
