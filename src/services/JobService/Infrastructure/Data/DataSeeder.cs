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
            // ── Google (employer 1) ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000001"),
                EmployerId = "e1000000-0000-0000-0000-000000000001",
                Title = "Senior Software Engineer, Cloud Infrastructure",
                Description = "Google Cloud is building the future of enterprise computing, and we need experienced engineers to help us scale. As a Senior Software Engineer on the Cloud Infrastructure team, you will design, develop, and operate large-scale distributed systems that power Google Cloud Platform services used by millions of developers worldwide.\n\nYou'll work at the intersection of systems programming and cloud-native architecture, building the foundational services that other teams depend on. Our systems handle petabytes of data and millions of requests per second — reliability and performance are not optional.\n\nWhat you'll do:\n- Design and implement core infrastructure services in Go and C++ that run at Google scale\n- Lead technical design reviews and mentor engineers across the organization\n- Develop automation and tooling to improve developer productivity and system reliability\n- Collaborate with product managers and SREs to define SLOs and ensure we meet them\n- Drive incident response and conduct blameless postmortems\n\nWhat we're looking for:\n- 5+ years of experience building production distributed systems\n- Deep understanding of networking, storage, and compute fundamentals\n- Track record of leading cross-team technical initiatives\n- Experience with containerization and orchestration at scale",
                Location = "Mountain View, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 5,
                SalaryMin = 185000m,
                SalaryMax = 280000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-14),
                ApplicationDeadline = DateTime.UtcNow.AddDays(30),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Go"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Kubernetes"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["AWS"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["Terraform"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Leadership"], ImportanceLevel = ImportanceLevel.Required },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000002"),
                EmployerId = "e1000000-0000-0000-0000-000000000001",
                Title = "Software Engineer, Early Career — YouTube",
                Description = "YouTube reaches over 2 billion logged-in users every month, and our engineering teams build the features that keep creators and viewers connected. We're looking for early-career software engineers who are eager to learn, ship fast, and make an outsized impact on products used by people around the world.\n\nAs part of the YouTube Creator Tools team, you'll build the dashboards, analytics, and content management features that creators use to grow their channels and engage their audiences. This is a high-visibility product area with tight feedback loops — you'll see the impact of your work within days of launch.\n\nWhat you'll do:\n- Build responsive, accessible web interfaces using React and TypeScript\n- Write unit and integration tests to ensure feature quality before release\n- Participate in code reviews and design discussions with senior engineers\n- Collaborate with UX researchers and designers to ship intuitive creator experiences\n- Monitor feature performance using internal analytics and A/B testing frameworks\n\nWhat we're looking for:\n- BS/MS in Computer Science or equivalent practical experience\n- Familiarity with modern JavaScript frameworks (React preferred)\n- Strong fundamentals in data structures, algorithms, and web technologies\n- Genuine curiosity and a desire to learn from experienced engineers",
                Location = "San Bruno, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 1,
                SalaryMin = 118000m,
                SalaryMax = 170000m,
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
                Title = "Site Reliability Engineer, Google Cloud",
                Description = "Google's SRE teams are responsible for keeping the most heavily-trafficked services on the internet running smoothly. As an SRE on the Google Cloud team, you'll be the bridge between software engineering and operations — writing code to automate away toil, building monitoring and alerting systems, and designing for resilience at planetary scale.\n\nOur SREs don't just respond to incidents — they engineer systems that prevent them. You'll spend roughly half your time on engineering projects (automation, capacity planning tools, reliability improvements) and half on operational work (on-call, incident response, change management).\n\nWhat you'll do:\n- Build and maintain CI/CD pipelines that deploy to thousands of machines globally\n- Design monitoring, alerting, and self-healing infrastructure using internal and open-source tools\n- Lead incident response for tier-1 services and author detailed postmortems\n- Implement infrastructure-as-code for multi-region Kubernetes deployments\n- Collaborate with development teams to set and meet reliability targets (SLOs/SLIs)\n\nWhat we're looking for:\n- 3+ years of experience in SRE, DevOps, or systems engineering\n- Proficiency with container orchestration (Kubernetes), CI/CD systems, and IaC (Terraform)\n- Strong scripting skills in Python or Go for automation\n- Experience with cloud platforms (GCP, AWS, or Azure) in production environments",
                Location = "Mountain View, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 3,
                SalaryMin = 150000m,
                SalaryMax = 230000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-21),
                ApplicationDeadline = DateTime.UtcNow.AddDays(14),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Kubernetes"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["AWS"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Terraform"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["CI/CD"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Python"], ImportanceLevel = ImportanceLevel.Preferred },
                },
            },

            // ── Stripe (employer 2) ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000004"),
                EmployerId = "e1000000-0000-0000-0000-000000000002",
                Title = "Backend Engineer, Payments Platform",
                Description = "Stripe processes hundreds of billions of dollars in payments every year, and our Payments Platform team builds the core APIs and systems that make it all work. As a Backend Engineer, you'll design and build the services that millions of businesses rely on to accept payments, manage subscriptions, and move money around the world.\n\nThis is a high-stakes, low-latency environment where correctness matters as much as speed. You'll work on systems that must be exactly right — a misplaced decimal or a dropped event can have real financial consequences. We take testing, observability, and code quality seriously.\n\nWhat you'll do:\n- Design and implement payment processing APIs handling millions of transactions per day\n- Build event-driven microservices for payment lifecycle management (authorizations, captures, refunds)\n- Optimize database query performance for high-throughput financial operations\n- Write comprehensive integration tests that simulate real-world payment flows\n- Participate in on-call rotations and contribute to incident response processes\n\nWhat we're looking for:\n- 3+ years of backend engineering experience, ideally in fintech or payments\n- Strong proficiency in Python, Ruby, or Java with a willingness to learn our stack\n- Deep understanding of relational databases, transaction isolation, and data consistency\n- Experience building and operating production microservices",
                Location = "San Francisco, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 3,
                SalaryMin = 160000m,
                SalaryMax = 240000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-10),
                ApplicationDeadline = DateTime.UtcNow.AddDays(35),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Python"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["PostgreSQL"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Redis"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["SQL"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Communication"], ImportanceLevel = ImportanceLevel.Required },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000005"),
                EmployerId = "e1000000-0000-0000-0000-000000000002",
                Title = "Full-Stack Engineer, Stripe Dashboard",
                Description = "The Stripe Dashboard is the control center for millions of businesses — it's where they view transactions, manage disputes, configure webhooks, and analyze revenue. As a Full-Stack Engineer on the Dashboard team, you'll build the features that business owners interact with every day.\n\nOur frontend is a large-scale React application with a sophisticated design system, and our backend is a set of Ruby and Java services that aggregate data from across Stripe's infrastructure. You'll work across the entire stack, from pixel-perfect UI components to high-performance API endpoints.\n\nWhat you'll do:\n- Build and ship user-facing features end-to-end, from database schema to React component\n- Develop performant API endpoints that aggregate data from multiple backend services\n- Contribute to our shared component library and design system\n- Work with data scientists to build analytics and reporting features\n- Optimize page load times and runtime performance for a global user base\n\nWhat we're looking for:\n- 4+ years of experience building production web applications\n- Expert-level React and TypeScript skills with a strong design sensibility\n- Experience with server-side technologies (Java, Ruby, Python, or similar)\n- Familiarity with PostgreSQL or other relational databases at scale",
                Location = "Remote",
                EmploymentType = EmploymentType.Remote,
                ExperienceRequired = 4,
                SalaryMin = 170000m,
                SalaryMax = 250000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-5),
                ApplicationDeadline = DateTime.UtcNow.AddDays(40),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["React"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["TypeScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Java"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["PostgreSQL"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["Problem Solving"], ImportanceLevel = ImportanceLevel.Required },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000006"),
                EmployerId = "e1000000-0000-0000-0000-000000000002",
                Title = "Frontend Engineer, Stripe Elements",
                Description = "Stripe Elements is our suite of pre-built UI components that developers embed directly into their checkout flows. Used by hundreds of thousands of businesses, Elements must be fast, accessible, and work flawlessly across every browser, device, and payment method.\n\nAs a Frontend Engineer on the Elements team, you'll build and maintain the components that handle sensitive payment data in real-time. This is one of the most technically demanding frontend roles at Stripe — you'll deal with iframe security boundaries, PCI compliance constraints, cross-browser rendering, and sub-100ms interaction targets.\n\nWhat you'll do:\n- Build and maintain embeddable payment UI components used by millions of end users\n- Ensure full accessibility (WCAG 2.1 AA) and internationalization across 40+ locales\n- Optimize rendering performance for low-powered mobile devices\n- Write extensive cross-browser test suites and visual regression tests\n- Collaborate with the design system team to maintain visual consistency\n\nWhat we're looking for:\n- 2+ years of professional frontend development experience\n- Deep expertise in React, TypeScript, and modern CSS\n- Understanding of web security (CSP, iframe sandboxing, XSS prevention)\n- Experience with performance profiling and optimization",
                Location = "Seattle, WA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 2,
                SalaryMin = 140000m,
                SalaryMax = 200000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-3),
                ApplicationDeadline = DateTime.UtcNow.AddDays(50),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["React"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["TypeScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Next.js"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["JavaScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Problem Solving"], ImportanceLevel = ImportanceLevel.Preferred },
                },
            },

            // ── Amazon (employer 3) ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000007"),
                EmployerId = "e1000000-0000-0000-0000-000000000003",
                Title = "Software Development Engineer II, AWS Lambda",
                Description = "AWS Lambda serves billions of invocations per day and is the foundation of serverless computing on AWS. As an SDE II on the Lambda team, you'll work on the compute platform that lets developers run code without thinking about servers — and you'll do it at a scale that very few engineering teams in the world ever encounter.\n\nYou'll be responsible for the systems that cold-start functions in milliseconds, route traffic across availability zones, and scale from zero to thousands of concurrent executions seamlessly. This is deep systems work with real-world impact on every AWS customer.\n\nWhat you'll do:\n- Design and implement core Lambda runtime components in Go and Rust\n- Build and optimize container orchestration systems for sub-100ms cold starts\n- Develop internal tools for capacity planning, load testing, and performance analysis\n- Participate in operational reviews and drive improvements to service reliability\n- Mentor junior engineers and contribute to the team's technical roadmap\n\nWhat we're looking for:\n- 4+ years of software development experience with a strong systems background\n- Proficiency in Go, Rust, or C++ for systems-level programming\n- Experience with distributed systems, container runtimes, or virtualization\n- Demonstrated ability to own and operate production services at scale",
                Location = "Seattle, WA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 4,
                SalaryMin = 165000m,
                SalaryMax = 250000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-8),
                ApplicationDeadline = DateTime.UtcNow.AddDays(30),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Go"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Rust"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Kubernetes"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["AWS"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Terraform"], ImportanceLevel = ImportanceLevel.NiceToHave },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000008"),
                EmployerId = "e1000000-0000-0000-0000-000000000003",
                Title = "Software Development Engineer, Amazon Retail — .NET",
                Description = "Amazon's retail platform serves hundreds of millions of customers and processes orders worth billions of dollars every quarter. Our .NET engineering team builds the backend services that power product catalog management, inventory tracking, and order fulfillment across Amazon's global marketplace.\n\nAs an SDE on the Retail Platform team, you'll work on high-throughput C# microservices running on ASP.NET Core, backed by PostgreSQL and SQL Server databases. You'll design APIs consumed by dozens of internal teams and optimize systems that must handle Black Friday-level traffic spikes without breaking a sweat.\n\nWhat you'll do:\n- Build and maintain RESTful APIs and event-driven microservices using ASP.NET Core\n- Design and optimize database schemas for high-write, high-read workloads\n- Implement distributed caching strategies with Redis to reduce latency\n- Write comprehensive unit and integration tests with a focus on edge cases\n- Conduct code reviews and contribute to team coding standards and best practices\n\nWhat we're looking for:\n- 3+ years of professional experience with C# and the .NET ecosystem\n- Strong understanding of relational database design and query optimization\n- Experience building RESTful APIs consumed by multiple client teams\n- Familiarity with message queues, event-driven architectures, and caching patterns",
                Location = "Seattle, WA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 3,
                SalaryMin = 145000m,
                SalaryMax = 220000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-12),
                ApplicationDeadline = DateTime.UtcNow.AddDays(25),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["C#"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["ASP.NET Core"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["PostgreSQL"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["SQL Server"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Redis"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.NiceToHave },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000009"),
                EmployerId = "e1000000-0000-0000-0000-000000000003",
                Title = "Software Development Engineer Intern, AWS",
                Description = "Spend 12 weeks building real features on one of the most impactful cloud platforms in the world. As an SDE Intern at AWS, you won't be writing throwaway code or building demo apps — you'll ship production features used by AWS customers, with the mentorship and support of a senior engineer dedicated to your growth.\n\nInterns at AWS are placed on a specific team and given an ownership-level project that they design, implement, test, and deploy during their internship. Past intern projects have launched as GA features, and many interns return as full-time engineers.\n\nWhat you'll do:\n- Own a meaningful project from design through deployment on a real AWS service team\n- Write production-quality code in Python, Java, or Go (depending on your team)\n- Participate in code reviews, design discussions, and team standups\n- Present your work to the broader organization at the end of the internship\n- Learn AWS services hands-on by building on the platform you're helping to create\n\nWhat we're looking for:\n- Currently pursuing a BS/MS in Computer Science or related field (graduating 2026-2027)\n- Solid foundation in data structures, algorithms, and object-oriented programming\n- Familiarity with at least one programming language (Python, Java, Go, or similar)\n- Genuine curiosity about cloud computing and distributed systems",
                Location = "Seattle, WA",
                EmploymentType = EmploymentType.Internship,
                ExperienceRequired = 0,
                SalaryMin = 42000m,
                SalaryMax = 52000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-2),
                ApplicationDeadline = DateTime.UtcNow.AddDays(60),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Python"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Java"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["AWS"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["Teamwork"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Problem Solving"], ImportanceLevel = ImportanceLevel.Required },
                },
            },

            // ── Meta (employer 4) ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000010"),
                EmployerId = "e1000000-0000-0000-0000-000000000004",
                Title = "Production Engineer, Instagram",
                Description = "Instagram serves over 2 billion monthly active users, and our Production Engineering team is responsible for keeping it fast, reliable, and available 24/7. Production Engineers at Meta sit at the intersection of software and systems engineering — we write code to solve operational problems at massive scale.\n\nAs a Production Engineer on the Instagram Infrastructure team, you'll build and maintain the systems that serve the Instagram feed, stories, reels, and messaging to users around the world. You'll work on performance optimization, capacity planning, and automated remediation systems that keep Instagram running even when things go wrong.\n\nWhat you'll do:\n- Build monitoring, alerting, and automated remediation systems for Instagram's backend\n- Optimize Django and Python services for throughput and latency at Instagram scale\n- Design and implement load balancing and traffic management solutions\n- Develop capacity planning models and tooling for infrastructure provisioning\n- Participate in a 24/7 on-call rotation and lead incident response for critical services\n\nWhat we're looking for:\n- 2+ years of experience in production engineering, SRE, or backend development\n- Strong Python skills and experience with Django or similar web frameworks\n- Understanding of Linux systems, networking, and distributed systems fundamentals\n- Experience with monitoring/observability tools (Prometheus, Grafana, or similar)",
                Location = "Menlo Park, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 2,
                SalaryMin = 148000m,
                SalaryMax = 215000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-1),
                ApplicationDeadline = DateTime.UtcNow.AddDays(30),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Python"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Django"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["CI/CD"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Communication"], ImportanceLevel = ImportanceLevel.Required },
                },
            },

            // ── Microsoft (employer 5) ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000013"),
                EmployerId = "e1000000-0000-0000-0000-000000000005",
                Title = "Software Engineer, Azure DevOps",
                Description = "Azure DevOps is used by millions of developers to plan, build, test, and deploy software. As a Software Engineer on the Azure DevOps team, you'll build the CI/CD pipelines, artifact management, and developer collaboration tools that power some of the largest engineering organizations in the world.\n\nOur tech stack is primarily C# and ASP.NET Core on the backend with React and TypeScript on the frontend, running on Azure's global infrastructure. You'll work on features used by teams ranging from two-person startups to Fortune 500 enterprises.\n\nWhat you'll do:\n- Design and implement new features for Azure Pipelines and Azure Repos\n- Build scalable C# microservices that handle millions of CI/CD pipeline executions daily\n- Develop React/TypeScript UI components for the Azure DevOps web portal\n- Write comprehensive automated tests and contribute to our testing frameworks\n- Collaborate with PM and design partners to define product roadmap priorities\n\nWhat we're looking for:\n- 3+ years of experience building web applications or cloud services\n- Strong proficiency in C# and the .NET ecosystem\n- Experience with React and TypeScript for frontend development\n- Understanding of CI/CD concepts and software delivery practices\n- Passion for developer tools and improving the developer experience",
                Location = "Redmond, WA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 3,
                SalaryMin = 140000m,
                SalaryMax = 215000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-6),
                ApplicationDeadline = DateTime.UtcNow.AddDays(35),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["C#"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["ASP.NET Core"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["React"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["TypeScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Azure"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["CI/CD"], ImportanceLevel = ImportanceLevel.Preferred },
                },
            },
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000014"),
                EmployerId = "e1000000-0000-0000-0000-000000000005",
                Title = "Part-Time Software Engineer, Microsoft Learn",
                Description = "Microsoft Learn is the free, interactive learning platform that helps millions of people build technical skills and earn certifications. We're looking for a part-time software engineer (20 hours/week) to help build and maintain the learning modules, hands-on labs, and assessment features that make Microsoft Learn one of the most popular developer education platforms on the web.\n\nThis is an ideal role for engineers who want meaningful, impactful work with schedule flexibility — whether you're balancing education, caregiving, or other commitments.\n\nWhat you'll do:\n- Build interactive code exercise components using React and TypeScript\n- Develop backend APIs in C# for content management and learner progress tracking\n- Write automated tests for learning module rendering and assessment scoring\n- Collaborate with content authors and instructional designers on technical requirements\n- Monitor and improve platform performance and accessibility\n\nWhat we're looking for:\n- 2+ years of software development experience\n- Proficiency in JavaScript/TypeScript and at least one backend language\n- Experience with modern frontend frameworks (React preferred)\n- Strong written communication skills for async collaboration",
                Location = "Remote",
                EmploymentType = EmploymentType.PartTime,
                ExperienceRequired = 2,
                SalaryMin = 65000m,
                SalaryMax = 85000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-4),
                ApplicationDeadline = DateTime.UtcNow.AddDays(30),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["JavaScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["TypeScript"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["React"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["C#"], ImportanceLevel = ImportanceLevel.NiceToHave },
                    new() { SkillId = skills["Communication"], ImportanceLevel = ImportanceLevel.Required },
                },
            },

            // ── Netflix (employer 6) ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000015"),
                EmployerId = "e1000000-0000-0000-0000-000000000006",
                Title = "Senior Backend Engineer, Content Delivery",
                Description = "Netflix streams over 1 billion hours of content every week to 260+ million members across 190 countries. As a Senior Backend Engineer on the Content Delivery team, you'll build the systems that decide how, when, and where content is cached and delivered — ensuring that every member gets a seamless, buffer-free viewing experience regardless of their location or device.\n\nOur backend services are built in Java with Spring Boot, backed by a mix of Cassandra, PostgreSQL, and Redis, and deployed across a global CDN with thousands of edge nodes. You'll work on problems that require deep thinking about distributed systems, caching strategies, and network optimization.\n\nWhat you'll do:\n- Design and implement content routing and caching services in Java and Spring Boot\n- Build real-time analytics pipelines for monitoring CDN health and video quality metrics\n- Optimize content placement algorithms to reduce latency and improve streaming quality\n- Develop tools for capacity planning across Netflix's global open-connect infrastructure\n- Lead technical design reviews and mentor engineers on distributed systems patterns\n\nWhat we're looking for:\n- 5+ years of backend engineering experience with Java or a similar JVM language\n- Deep understanding of distributed systems, caching, and networking\n- Experience with large-scale data processing and real-time analytics\n- Track record of technical leadership and cross-team collaboration",
                Location = "Los Gatos, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 5,
                SalaryMin = 200000m,
                SalaryMax = 350000m,
                JobStatus = JobStatus.Active,
                PostedAt = DateTime.UtcNow.AddDays(-9),
                ApplicationDeadline = DateTime.UtcNow.AddDays(28),
                JobSkills = new List<JobSkill>
                {
                    new() { SkillId = skills["Java"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Spring Boot"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["PostgreSQL"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Redis"], ImportanceLevel = ImportanceLevel.Required },
                    new() { SkillId = skills["Docker"], ImportanceLevel = ImportanceLevel.Preferred },
                    new() { SkillId = skills["Leadership"], ImportanceLevel = ImportanceLevel.Required },
                },
            },

            // ── Closed/expired jobs for stats variety ──
            new()
            {
                JobId = Guid.Parse("d0000000-0000-0000-0000-000000000011"),
                EmployerId = "e1000000-0000-0000-0000-000000000001",
                Title = "Mobile Developer (React Native) — Google Maps",
                Description = "We were looking for a React Native developer to build cross-platform features for Google Maps mobile clients. This position has been filled after a successful hire in Q1 2026.",
                Location = "Mountain View, CA",
                EmploymentType = EmploymentType.Contract,
                ExperienceRequired = 2,
                SalaryMin = 140000m,
                SalaryMax = 190000m,
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
                Title = "Data Analyst, Stripe Revenue Analytics",
                Description = "This data analyst role on the Stripe Revenue Analytics team has been filled. The position involved building dashboards and models to help Stripe understand payment volume trends, merchant growth, and revenue forecasting.",
                Location = "San Francisco, CA",
                EmploymentType = EmploymentType.FullTime,
                ExperienceRequired = 1,
                SalaryMin = 110000m,
                SalaryMax = 155000m,
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
