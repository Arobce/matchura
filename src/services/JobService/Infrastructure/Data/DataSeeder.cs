using JobService.Domain.Entities;
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
}
