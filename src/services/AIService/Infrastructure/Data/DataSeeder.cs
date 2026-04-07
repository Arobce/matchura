using AIService.Domain.Entities;
using AIService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AIService.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AIDbContext db)
    {
        if (await db.CandidateSkills.AnyAsync()) return;

        // ── Candidate Skills ──
        var candidateSkills = new List<CandidateSkill>
        {
            // Alice — full-stack (React, C#, TypeScript, Docker, PostgreSQL)
            new() { CandidateId = "c1000000-0000-0000-0000-000000000001", SkillName = "C#", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 5, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000001", SkillName = "React", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 4, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000001", SkillName = "TypeScript", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 4, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000001", SkillName = "JavaScript", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 6, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000001", SkillName = "Node.js", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 3, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000001", SkillName = "Docker", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 2, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000001", SkillName = "PostgreSQL", SkillCategory = "Databases", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 4, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000001", SkillName = "ASP.NET Core", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 4, Source = "seed" },

            // Bob — backend Java/Spring
            new() { CandidateId = "c1000000-0000-0000-0000-000000000002", SkillName = "Java", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 4, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000002", SkillName = "Spring Boot", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 3, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000002", SkillName = "Kubernetes", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 2, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000002", SkillName = "PostgreSQL", SkillCategory = "Databases", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 3, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000002", SkillName = "Docker", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 2, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000002", SkillName = "SQL", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 4, Source = "seed" },

            // Carol — DevOps
            new() { CandidateId = "c1000000-0000-0000-0000-000000000003", SkillName = "Docker", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 5, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000003", SkillName = "Kubernetes", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 4, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000003", SkillName = "AWS", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 5, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000003", SkillName = "Terraform", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 3, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000003", SkillName = "CI/CD", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 5, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000003", SkillName = "Python", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 3, Source = "seed" },

            // David — Python/Data
            new() { CandidateId = "c1000000-0000-0000-0000-000000000004", SkillName = "Python", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 3, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000004", SkillName = "Django", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 3, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000004", SkillName = "PostgreSQL", SkillCategory = "Databases", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 3, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000004", SkillName = "Redis", SkillCategory = "Databases", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 2, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000004", SkillName = "SQL", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 3, Source = "seed" },

            // Emma — Frontend React
            new() { CandidateId = "c1000000-0000-0000-0000-000000000005", SkillName = "React", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 2, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000005", SkillName = "TypeScript", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 2, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000005", SkillName = "Next.js", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 1, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000005", SkillName = "JavaScript", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 2, Source = "seed" },

            // Frank — Senior C#/.NET
            new() { CandidateId = "c1000000-0000-0000-0000-000000000006", SkillName = "C#", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 8, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000006", SkillName = "ASP.NET Core", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 6, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000006", SkillName = "Azure", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 4, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000006", SkillName = "SQL Server", SkillCategory = "Databases", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 7, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000006", SkillName = "Docker", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 3, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000006", SkillName = "PostgreSQL", SkillCategory = "Databases", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 3, Source = "seed" },

            // Grace — Junior JS/React
            new() { CandidateId = "c1000000-0000-0000-0000-000000000007", SkillName = "JavaScript", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsUsed = 1, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000007", SkillName = "React", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Beginner, YearsUsed = 1, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000007", SkillName = "Node.js", SkillCategory = "Frameworks", ProficiencyLevel = ProficiencyLevel.Beginner, YearsUsed = 1, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000007", SkillName = "MongoDB", SkillCategory = "Databases", ProficiencyLevel = ProficiencyLevel.Beginner, YearsUsed = 1, Source = "seed" },

            // Henry — Cloud Architect (Go/Rust/AWS)
            new() { CandidateId = "c1000000-0000-0000-0000-000000000008", SkillName = "Go", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 5, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000008", SkillName = "Rust", SkillCategory = "Programming", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 3, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000008", SkillName = "AWS", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 7, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000008", SkillName = "Terraform", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 5, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000008", SkillName = "Kubernetes", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Advanced, YearsUsed = 4, Source = "seed" },
            new() { CandidateId = "c1000000-0000-0000-0000-000000000008", SkillName = "Docker", SkillCategory = "DevOps", ProficiencyLevel = ProficiencyLevel.Expert, YearsUsed = 6, Source = "seed" },
        };

        db.CandidateSkills.AddRange(candidateSkills);
        await db.SaveChangesAsync();

        // ── Match Scores ──
        var matchScores = new List<MatchScore>
        {
            // Alice → Senior Full-Stack (great match)
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000001",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000001"),
                OverallScore = 88.5m,
                SkillScore = 92.0m,
                ExperienceScore = 85.0m,
                EducationScore = 80.0m,
                Explanation = "Strong match. Has all required skills (C#, React, TypeScript) with 6 years of full-stack experience.",
                Strengths = "[\"Expert JavaScript/TypeScript\",\"Advanced C# and React\",\"Good PostgreSQL experience\",\"Docker familiarity\"]",
                Gaps = "[\"Could strengthen leadership experience\"]",
            },
            // Alice → .NET Backend
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000001",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000008"),
                OverallScore = 82.0m,
                SkillScore = 85.0m,
                ExperienceScore = 80.0m,
                EducationScore = 75.0m,
                Explanation = "Good match for .NET backend role. Strong C# and ASP.NET Core skills.",
                Strengths = "[\"Advanced C# and ASP.NET Core\",\"PostgreSQL experience\",\"Docker knowledge\"]",
                Gaps = "[\"Limited SQL Server experience\",\"No Azure exposure\"]",
            },
            // Bob → Backend Java
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000002",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000005"),
                OverallScore = 94.0m,
                SkillScore = 96.0m,
                ExperienceScore = 90.0m,
                EducationScore = 90.0m,
                Explanation = "Excellent match. Expert Java with Spring Boot, exactly what this role needs. K8s is a bonus.",
                Strengths = "[\"Expert Java developer\",\"Strong Spring Boot experience\",\"PostgreSQL proficiency\",\"Kubernetes exposure\"]",
                Gaps = "[]",
            },
            // Carol → DevOps
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000003",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000003"),
                OverallScore = 96.0m,
                SkillScore = 98.0m,
                ExperienceScore = 95.0m,
                EducationScore = 85.0m,
                Explanation = "Near-perfect match. Expert in all required DevOps skills with 5 years experience.",
                Strengths = "[\"Expert Docker, Kubernetes, AWS, CI/CD\",\"Advanced Terraform\",\"Python scripting ability\"]",
                Gaps = "[]",
            },
            // David → Python Data Engineer
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000004",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000004"),
                OverallScore = 91.0m,
                SkillScore = 93.0m,
                ExperienceScore = 88.0m,
                EducationScore = 85.0m,
                Explanation = "Excellent match. Strong Python, Django, and PostgreSQL with relevant data pipeline experience.",
                Strengths = "[\"Advanced Python and Django\",\"PostgreSQL expertise\",\"Redis experience\",\"Data Science background\"]",
                Gaps = "[\"Docker is nice-to-have, limited exposure\"]",
            },
            // Emma → Junior Frontend
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000005",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000002"),
                OverallScore = 78.0m,
                SkillScore = 82.0m,
                ExperienceScore = 75.0m,
                EducationScore = 70.0m,
                Explanation = "Good match for junior role. Has React and TypeScript, meets experience requirements.",
                Strengths = "[\"Advanced React\",\"TypeScript proficiency\",\"Next.js exposure\"]",
                Gaps = "[\"Limited professional experience\",\"No testing framework listed\"]",
            },
            // Frank → Senior Full-Stack
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000006",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000001"),
                OverallScore = 85.0m,
                SkillScore = 80.0m,
                ExperienceScore = 95.0m,
                EducationScore = 90.0m,
                Explanation = "Very experienced candidate. Expert C# but React skills not listed — may need ramp-up on frontend.",
                Strengths = "[\"Expert C# with 8 years\",\"Strong ASP.NET Core\",\"Leadership experience\",\"PostgreSQL and SQL Server\"]",
                Gaps = "[\"React not listed as a skill\",\"TypeScript proficiency unknown\"]",
            },
            // Henry → Cloud Platform (Go)
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000008",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000007"),
                OverallScore = 95.0m,
                SkillScore = 97.0m,
                ExperienceScore = 93.0m,
                EducationScore = 90.0m,
                Explanation = "Outstanding match. Expert Go developer with deep K8s, AWS, and Terraform experience. Rust is a bonus.",
                Strengths = "[\"Expert Go developer\",\"Deep Kubernetes experience\",\"AWS and Terraform expert\",\"Rust knowledge for performance-critical work\"]",
                Gaps = "[]",
            },
            // Grace → Junior Frontend (weaker match)
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000007",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000002"),
                OverallScore = 55.0m,
                SkillScore = 50.0m,
                ExperienceScore = 60.0m,
                EducationScore = 45.0m,
                Explanation = "Entry-level candidate. Has basic JavaScript and React from bootcamp but limited depth.",
                Strengths = "[\"JavaScript fundamentals\",\"React basics\",\"Eager to learn\"]",
                Gaps = "[\"No TypeScript experience\",\"Beginner-level React\",\"No formal CS degree\"]",
            },
            // Bob → Python Data Engineer (cross-domain)
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000002",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000004"),
                OverallScore = 45.0m,
                SkillScore = 35.0m,
                ExperienceScore = 55.0m,
                EducationScore = 60.0m,
                Explanation = "Weak match. Primarily a Java developer with limited Python and no Django experience.",
                Strengths = "[\"SQL proficiency\",\"PostgreSQL experience\",\"Docker familiarity\"]",
                Gaps = "[\"No Python expertise\",\"No Django experience\",\"No Redis\"]",
            },
        };

        db.MatchScores.AddRange(matchScores);
        await db.SaveChangesAsync();

        // ── Skill Gap Reports ──
        var skillGapReports = new List<SkillGapReport>
        {
            // Alice analyzing gap for Senior Full-Stack
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000001",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000001"),
                Summary = "Alice is well-prepared for this role. Minor gap in leadership skills, but technical skills are strong across all requirements.",
                OverallReadiness = 88.0m,
                EstimatedTimeToReady = "2 weeks",
                MissingSkills = "[{\"skillName\":\"Leadership\",\"currentLevel\":\"None\",\"requiredLevel\":\"NiceToHave\",\"priority\":\"Low\",\"estimatedWeeksToLearn\":2}]",
                RecommendedActions = "[{\"action\":\"Take a tech lead workshop or mentor junior developers\",\"priority\":\"Low\",\"estimatedDuration\":\"2 weeks\",\"resourceUrl\":\"https://www.coursera.org/learn/tech-leadership\"}]",
                StrengthAreas = "[\"C# (Advanced, 5 yrs)\",\"React (Advanced, 4 yrs)\",\"TypeScript (Advanced, 4 yrs)\",\"PostgreSQL (Advanced, 4 yrs)\",\"Docker (Intermediate, 2 yrs)\"]",
            },
            // Emma analyzing gap for Junior Frontend
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000005",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000002"),
                Summary = "Emma meets most requirements for this junior role. TypeScript is preferred but she already has intermediate proficiency. Next.js experience is a bonus she already has.",
                OverallReadiness = 78.0m,
                EstimatedTimeToReady = "4 weeks",
                MissingSkills = "[{\"skillName\":\"Testing Frameworks\",\"currentLevel\":\"None\",\"requiredLevel\":\"Preferred\",\"priority\":\"Medium\",\"estimatedWeeksToLearn\":3},{\"skillName\":\"Teamwork\",\"currentLevel\":\"None\",\"requiredLevel\":\"Required\",\"priority\":\"Medium\",\"estimatedWeeksToLearn\":1}]",
                RecommendedActions = "[{\"action\":\"Complete Jest/React Testing Library course\",\"priority\":\"Medium\",\"estimatedDuration\":\"3 weeks\",\"resourceUrl\":\"https://testing-library.com/docs/react-testing-library/intro/\"},{\"action\":\"Contribute to open-source React projects for collaboration experience\",\"priority\":\"Medium\",\"estimatedDuration\":\"Ongoing\",\"resourceUrl\":\"https://github.com/topics/react\"}]",
                StrengthAreas = "[\"React (Advanced, 2 yrs)\",\"TypeScript (Intermediate, 2 yrs)\",\"Next.js (Intermediate, 1 yr)\",\"JavaScript (Advanced, 2 yrs)\"]",
            },
            // Bob analyzing gap for Python Data Engineer
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000002",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000004"),
                Summary = "Bob's Java backend skills are strong but this role requires Python expertise he doesn't have. Significant ramp-up needed in Python ecosystem.",
                OverallReadiness = 40.0m,
                EstimatedTimeToReady = "12 weeks",
                MissingSkills = "[{\"skillName\":\"Python\",\"currentLevel\":\"None\",\"requiredLevel\":\"Required\",\"priority\":\"Critical\",\"estimatedWeeksToLearn\":8},{\"skillName\":\"Django\",\"currentLevel\":\"None\",\"requiredLevel\":\"Preferred\",\"priority\":\"High\",\"estimatedWeeksToLearn\":6},{\"skillName\":\"Redis\",\"currentLevel\":\"None\",\"requiredLevel\":\"Preferred\",\"priority\":\"Medium\",\"estimatedWeeksToLearn\":2}]",
                RecommendedActions = "[{\"action\":\"Complete Python for experienced developers course\",\"priority\":\"Critical\",\"estimatedDuration\":\"6 weeks\",\"resourceUrl\":\"https://docs.python.org/3/tutorial/\"},{\"action\":\"Build a Django REST API project\",\"priority\":\"High\",\"estimatedDuration\":\"4 weeks\",\"resourceUrl\":\"https://www.django-rest-framework.org/tutorial/quickstart/\"},{\"action\":\"Redis fundamentals course\",\"priority\":\"Medium\",\"estimatedDuration\":\"1 week\",\"resourceUrl\":\"https://redis.io/docs/getting-started/\"}]",
                StrengthAreas = "[\"SQL (Advanced, 4 yrs)\",\"PostgreSQL (Advanced, 3 yrs)\",\"Docker (Intermediate, 2 yrs)\"]",
            },
            // Henry analyzing gap for Cloud Platform (Go) — near-perfect
            new()
            {
                CandidateId = "c1000000-0000-0000-0000-000000000008",
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000007"),
                Summary = "Henry is an exceptional match for this role. Expert Go developer with deep cloud infrastructure experience. No significant skill gaps.",
                OverallReadiness = 95.0m,
                EstimatedTimeToReady = "Ready now",
                MissingSkills = "[]",
                RecommendedActions = "[{\"action\":\"Review CloudNine's specific K8s operator patterns\",\"priority\":\"Low\",\"estimatedDuration\":\"1 week\",\"resourceUrl\":\"https://kubernetes.io/docs/concepts/extend-kubernetes/operator/\"}]",
                StrengthAreas = "[\"Go (Expert, 5 yrs)\",\"Kubernetes (Advanced, 4 yrs)\",\"Docker (Expert, 6 yrs)\",\"AWS (Expert, 7 yrs)\",\"Terraform (Expert, 5 yrs)\",\"Rust (Advanced, 3 yrs)\"]",
            },
        };

        db.SkillGapReports.AddRange(skillGapReports);
        await db.SaveChangesAsync();
    }
}
