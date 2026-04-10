# Matchura Technical Documentation

## Table of Contents

1. [Project Overview](#project-overview)
2. [System Architecture](#system-architecture)
3. [Technology Stack](#technology-stack)
4. [Microservices Breakdown](#microservices-breakdown)
5. [Database Design](#database-design)
6. [AI Agent System](#ai-agent-system)
7. [Event-Driven Communication](#event-driven-communication)
8. [Authentication and Authorization](#authentication-and-authorization)
9. [API Gateway](#api-gateway)
10. [Frontend Application](#frontend-application)
11. [Testing Strategy](#testing-strategy)
12. [CI/CD Pipeline](#cicd-pipeline)
13. [Containerization](#containerization)
14. [Monitoring and Observability](#monitoring-and-observability)
15. [Deployment Instructions](#deployment-instructions)

---

## Project Overview

Matchura is a job matching platform that connects candidates with employers using AI to automate resume screening, match scoring, and skill gap analysis. The platform is built as a distributed system using a microservices architecture where each service owns its data, communicates through events, and can be deployed independently.

The core idea is simple: when a candidate uploads a resume, the system parses it into structured data using Claude (Anthropic's LLM). When an employer publishes a job, the system automatically computes match scores for every candidate and notifies them in real time. Employers see ranked candidates with detailed breakdowns of why each one is or is not a good fit.

The project was built as a capstone project to demonstrate real-world distributed systems design, including event-driven architecture, background job processing, AI integration, and production deployment on Railway.

---

## System Architecture

Matchura follows the microservices pattern with a database-per-service approach. Services communicate synchronously through HTTP for queries and asynchronously through RabbitMQ for events. A YARP-based API gateway sits in front of all services and handles routing, JWT validation, CORS, and rate limiting.

```
                         +--------------------+
                         |   Next.js Frontend |
                         |   React 19, Zustand|
                         +---------+----------+
                                   |
                         +---------v----------+
                         |    API Gateway     |
                         | YARP Reverse Proxy |
                         | JWT + Rate Limits  |
                         +---------+----------+
                                   |
          +----------+----------+--+--+----------+----------+
          |          |          |     |           |          |
     +----v---+ +---v----+ +--v--+ +-v-------+ +v-------+ +v----------+
     |  Auth  | |Profile | | Job | |  App    | | Notif  | |    AI     |
     |Service | |Service | |Svc  | | Service | | Service| |  Service  |
     +----+---+ +---+----+ +--+--+ +----+----+ +---+----+ +-----+----+
          |         |         |         |           |            |
     +----v---+ +---v----+ +--v---+ +--v------+ +--v-----+ +---v-----+
     |auth_db | |prof_db | |job_db| |app_db   | |notif_db| |  ai_db  |
     +--------+ +--------+ +------+ +---------+ +--------+ +---------+

                    PostgreSQL 16 (one DB per service)

              +-------------+            +-----------+
              |  RabbitMQ 4 |            |  Redis 7  |
              |  Event Bus  |            |   Cache   |
              +-------------+            +-----------+

              +-------------+
              |   AWS S3    |
              |  Resumes    |
              +-------------+
```

**Key architectural decisions:**

- Each service has its own PostgreSQL database. There are no cross-database joins. Services reference each other's entities by ID only.
- RabbitMQ handles async event propagation (e.g., a published job triggers matching across all candidates).
- Redis caches expensive AI results like match scores so the system does not re-call the Claude API unnecessarily.
- The API gateway validates JWT tokens before forwarding requests, so individual services trust the gateway's authentication.

---

## Technology Stack

### Backend

| Tool | Version | Purpose |
|------|---------|---------|
| .NET | 10 | Runtime for all microservices |
| ASP.NET Core | 10 | Web framework, controllers, middleware |
| Entity Framework Core | 10 | ORM, migrations, database access |
| PostgreSQL | 16 | Primary data store (6 databases) |
| Redis | 7 | Caching layer for match scores |
| RabbitMQ | 4 | Message broker for async events |
| YARP | 2.3.0 | Reverse proxy for API gateway |
| FluentValidation | 11.3.1 | Request validation |
| ASP.NET Identity | 10 | User management, password hashing, roles |
| MailKit | 4.15.1 | SMTP email delivery |
| Claude API | 2023-06-01 | LLM for resume parsing and matching |
| AWS S3 SDK | 4.0.21 | Resume file storage |
| UglyToad.PdfPig | 1.7.0 | PDF text extraction |
| DocumentFormat.OpenXml | 3.5.1 | DOCX text extraction |
| SignalR | 10 | WebSocket real-time notifications |

### Frontend

| Tool | Version | Purpose |
|------|---------|---------|
| Next.js | 16 | React framework with SSR |
| React | 19 | UI library |
| TypeScript | 5 | Type safety |
| Tailwind CSS | 4 | Utility-first styling |
| Zustand | 5.0.12 | Lightweight state management |
| React Hook Form | 7.72.1 | Form handling |
| Zod | 4.3.6 | Schema validation |
| SignalR JS Client | 10.0.0 | Real-time WebSocket client |
| Recharts | 3.8.1 | Data visualization |
| @dnd-kit | 6.3.1 | Drag and drop interactions |
| Lucide React | 1.7.0 | Icon library |

### Infrastructure and Tooling

| Tool | Purpose |
|------|---------|
| Docker | Containerization for all services |
| GitHub Actions | CI/CD pipeline |
| Railway | Production hosting |
| Sentry | Error tracking and distributed tracing |
| Playwright | End-to-end browser testing |
| Vitest | Frontend unit and component testing |
| xUnit | Backend unit and integration testing |
| pgAdmin | Database management UI |

---

## Microservices Breakdown

### AuthService (Port 5001)

Handles user registration, login, email verification, and two-factor authentication. Built on top of ASP.NET Identity for password hashing, lockout policies, and role management.

**Responsibilities:**
- User registration with email uniqueness and password complexity validation
- JWT token generation with configurable issuer, audience, and expiry
- Email verification via 6-digit codes sent through SendLayer SMTP
- Two-factor authentication via email codes
- Role seeding on startup (Candidate, Employer, Admin)
- Test user seeding for development

### ProfileService (Port 5002)

Manages candidate and employer profile data. Each user has exactly one profile linked by their auth user ID.

**Responsibilities:**
- Candidate profiles: phone, location, professional summary, years of experience, education, LinkedIn URL
- Employer profiles: company name, description, industry, website, location, logo URL
- Profile CRUD with ownership validation (users can only edit their own profiles)

### JobService (Port 5003)

Handles job posting lifecycle and maintains the skill taxonomy. When a job is published, it emits a `JobPublishedEvent` through RabbitMQ so other services can react.

**Responsibilities:**
- Job CRUD with status management (Draft, Active, Closed, Expired)
- Skill taxonomy with categories: Programming, Framework, Database, DevOps, Cloud, Design, Soft Skills
- Many-to-many job-skill relationships with importance levels (Required, Preferred, Nice to Have)
- Publishing a job emits `JobPublishedEvent` for downstream processing
- Seed data for common skills and sample jobs

### ApplicationService (Port 5004)

Tracks job applications through their lifecycle. Prevents duplicate applications from the same candidate to the same job.

**Responsibilities:**
- Application submission with cover letter and resume URL
- Status tracking: Submitted, Reviewed, Shortlisted, Accepted, Rejected, Withdrawn
- Employer notes and feedback on applications
- Fetches job details from JobService via HTTP for denormalized fields

### AIService (Port 5005)

The core intelligence layer. Uses Claude (Anthropic's LLM) through three specialized agents to parse resumes, compute match scores, and analyze skill gaps. Heavy operations run in background workers so API responses stay fast.

**Responsibilities:**
- Resume upload to AWS S3 with PDF/DOCX text extraction
- Background resume parsing via Claude (structured JSON output)
- Match score computation with skill, experience, and education breakdowns
- Skill gap analysis with learning recommendations
- Redis caching for match scores
- Consumes `JobPublishedEvent` to auto-match all candidates when a job goes live

### NotificationService (Port 5006)

Delivers real-time notifications to users through a SignalR WebSocket hub. Listens to RabbitMQ events and pushes updates to connected clients.

**Responsibilities:**
- SignalR hub at `/notifications-hub` with JWT authentication
- User group-based notification delivery
- Notification persistence with read/unread tracking
- Consumes events from RabbitMQ: application status changes, job matches

### ApiGateway (Port 5010)

Single entry point for all client requests. Routes traffic to the correct service, validates JWT tokens, enforces rate limits, and monitors downstream service health.

**Responsibilities:**
- YARP reverse proxy routing to all 6 services
- JWT validation before forwarding authenticated requests
- Three-tier rate limiting: anonymous (30/min), authenticated (100/min), AI endpoints (20/min)
- CORS configuration for the frontend origin
- Health check aggregation for all downstream services
- Request logging (method, path, status code, duration)

---

## Database Design

Each service owns a dedicated PostgreSQL database. There are no foreign keys across databases. Services reference external entities by string or UUID identifiers and keep their own copies of data they need frequently (denormalization).

### auth_db

Built on ASP.NET Identity. The `AspNetUsers` table is extended with custom fields.

**AspNetUsers**

| Column | Type | Constraints |
|--------|------|-------------|
| Id | text | PK |
| FullName | varchar(100) | NOT NULL |
| AccountStatus | text | NOT NULL (Active, Suspended, Deactivated) |
| Email | varchar(256) | Indexed |
| EmailConfirmed | boolean | NOT NULL |
| EmailVerificationCode | varchar(6) | Nullable |
| EmailVerificationCodeExpiry | timestamp | Nullable |
| TwoFactorEmailEnabled | boolean | NOT NULL |
| TwoFactorEmailCode | varchar(6) | Nullable |
| TwoFactorEmailCodeExpiry | timestamp | Nullable |
| PasswordHash | text | Nullable |
| CreatedAt | timestamp | NOT NULL, defaults to UTC now |
| UpdatedAt | timestamp | NOT NULL |

Also includes standard Identity tables: AspNetRoles, AspNetUserRoles, AspNetUserClaims, AspNetUserLogins, AspNetUserTokens, AspNetRoleClaims.

**Seeded Roles:** Candidate, Employer, Admin

### profile_db

**CandidateProfiles**

| Column | Type | Constraints |
|--------|------|-------------|
| CandidateId | uuid | PK, auto-generated |
| UserId | varchar(450) | NOT NULL, unique index |
| Phone | varchar(20) | Nullable |
| Location | varchar(200) | Nullable |
| ProfessionalSummary | varchar(2000) | Nullable |
| YearsOfExperience | integer | NOT NULL |
| HighestEducation | varchar(200) | Nullable |
| LinkedinUrl | varchar(500) | Nullable |
| CreatedAt | timestamp | NOT NULL |
| UpdatedAt | timestamp | NOT NULL |

**EmployerProfiles**

| Column | Type | Constraints |
|--------|------|-------------|
| EmployerId | uuid | PK, auto-generated |
| UserId | varchar(450) | NOT NULL, unique index |
| CompanyName | varchar(200) | NOT NULL |
| CompanyDescription | varchar(2000) | Nullable |
| Industry | varchar(100) | Nullable |
| WebsiteUrl | varchar(500) | Nullable |
| CompanyLocation | varchar(200) | Nullable |
| LogoUrl | varchar(500) | Nullable |
| CreatedAt | timestamp | NOT NULL |
| UpdatedAt | timestamp | NOT NULL |

### job_db

**Jobs**

| Column | Type | Constraints |
|--------|------|-------------|
| JobId | uuid | PK, auto-generated |
| EmployerId | varchar(450) | NOT NULL, indexed |
| Title | varchar(200) | NOT NULL |
| Description | varchar(5000) | NOT NULL |
| Location | varchar(200) | Nullable |
| EmploymentType | varchar(20) | NOT NULL (FullTime, PartTime, Contract, Internship, Remote) |
| ExperienceRequired | integer | NOT NULL |
| SalaryMin | decimal(18,2) | Nullable |
| SalaryMax | decimal(18,2) | Nullable |
| JobStatus | varchar(20) | NOT NULL (Draft, Active, Closed, Expired) |
| PostedAt | timestamp | NOT NULL |
| ApplicationDeadline | timestamp | Nullable |
| CreatedAt | timestamp | NOT NULL |
| UpdatedAt | timestamp | NOT NULL |

Composite index on `(JobStatus, PostedAt)` for filtering active jobs sorted by date.

**Skills**

| Column | Type | Constraints |
|--------|------|-------------|
| SkillId | uuid | PK, auto-generated |
| SkillName | varchar(100) | NOT NULL, unique index |
| SkillCategory | varchar(50) | Nullable |

Categories: Programming, Framework, Database, DevOps, Cloud, Design, Soft Skills

**JobSkills** (junction table)

| Column | Type | Constraints |
|--------|------|-------------|
| JobSkillId | uuid | PK, auto-generated |
| JobId | uuid | FK to Jobs, cascade delete |
| SkillId | uuid | FK to Skills, cascade delete |
| ImportanceLevel | varchar(20) | NOT NULL (Required, Preferred, NiceToHave) |

Unique composite index on `(JobId, SkillId)` to prevent duplicate skill assignments.

### application_db

**Applications**

| Column | Type | Constraints |
|--------|------|-------------|
| ApplicationId | uuid | PK, auto-generated |
| CandidateId | varchar(450) | NOT NULL, indexed |
| CandidateName | text | Nullable (denormalized) |
| JobId | uuid | NOT NULL, indexed |
| JobTitle | text | Nullable (denormalized) |
| CoverLetter | varchar(3000) | Nullable |
| CoverLetterUrl | varchar(500) | Nullable |
| ResumeUrl | varchar(500) | Nullable |
| Status | varchar(20) | NOT NULL (Submitted, Reviewed, Shortlisted, Accepted, Rejected, Withdrawn) |
| EmployerNotes | varchar(2000) | Nullable |
| AppliedAt | timestamp | NOT NULL |
| UpdatedAt | timestamp | NOT NULL |

Unique composite index on `(CandidateId, JobId)` to prevent a candidate from applying to the same job twice.

### ai_db

**Resumes**

| Column | Type | Constraints |
|--------|------|-------------|
| ResumeId | uuid | PK, auto-generated |
| CandidateId | varchar(450) | NOT NULL, indexed |
| OriginalFileName | varchar(255) | NOT NULL |
| FileUrl | varchar(500) | NOT NULL |
| ContentType | varchar(100) | NOT NULL |
| RawText | text | Nullable |
| ParsedData | jsonb | Nullable (structured resume data) |
| ParseStatus | varchar(20) | NOT NULL (Uploaded, Extracting, Parsing, Completed, Failed) |
| ErrorMessage | varchar(2000) | Nullable |
| UploadedAt | timestamp | NOT NULL |
| ParsedAt | timestamp | Nullable |

**CandidateSkills**

| Column | Type | Constraints |
|--------|------|-------------|
| CandidateSkillId | uuid | PK, auto-generated |
| CandidateId | varchar(450) | NOT NULL, indexed |
| SkillName | varchar(100) | NOT NULL |
| SkillCategory | varchar(50) | Nullable |
| ProficiencyLevel | varchar(20) | NOT NULL (Beginner, Intermediate, Advanced, Expert) |
| YearsUsed | integer | Nullable |
| Source | varchar(20) | NOT NULL (e.g., resume_parse) |

**MatchScores**

| Column | Type | Constraints |
|--------|------|-------------|
| MatchScoreId | uuid | PK, auto-generated |
| CandidateId | varchar(450) | NOT NULL |
| JobId | uuid | NOT NULL, indexed |
| OverallScore | decimal(5,2) | NOT NULL (0-100) |
| SkillScore | decimal(5,2) | NOT NULL |
| ExperienceScore | decimal(5,2) | NOT NULL |
| EducationScore | decimal(5,2) | NOT NULL |
| Explanation | varchar(2000) | Nullable |
| Strengths | jsonb | Nullable |
| Gaps | jsonb | Nullable |
| GeneratedAt | timestamp | NOT NULL |

Unique composite index on `(CandidateId, JobId)` so there is only one score per candidate-job pair.

**SkillGapReports**

| Column | Type | Constraints |
|--------|------|-------------|
| ReportId | uuid | PK, auto-generated |
| CandidateId | varchar(450) | NOT NULL |
| JobId | uuid | NOT NULL, indexed |
| Summary | varchar(2000) | Nullable |
| OverallReadiness | decimal(5,2) | NOT NULL (0-100) |
| EstimatedTimeToReady | varchar(50) | Nullable |
| MissingSkills | jsonb | Nullable |
| RecommendedActions | jsonb | Nullable |
| StrengthAreas | jsonb | Nullable |
| GeneratedAt | timestamp | NOT NULL |

Unique composite index on `(CandidateId, JobId)`.

### notification_db

**Notifications**

| Column | Type | Constraints |
|--------|------|-------------|
| NotificationId | uuid | PK, auto-generated |
| UserId | varchar(450) | NOT NULL |
| Type | varchar(50) | NOT NULL |
| Title | varchar(200) | NOT NULL |
| Message | varchar(1000) | NOT NULL |
| RelatedEntityId | varchar(100) | Nullable |
| RelatedEntityType | varchar(50) | Nullable |
| IsRead | boolean | NOT NULL |
| CreatedAt | timestamp | NOT NULL |

Composite indexes on `(UserId, CreatedAt)` for recent notifications and `(UserId, IsRead)` for unread counts.

### Cross-Service References

Since each service has its own database, there are no foreign keys between services. Instead, services store IDs that logically reference entities in other databases:

- `CandidateProfiles.UserId` and `EmployerProfiles.UserId` reference `AspNetUsers.Id` in auth_db
- `Jobs.EmployerId` references a user ID from auth_db
- `Applications.CandidateId` references a user ID from auth_db
- `Applications.JobId` references `Jobs.JobId` in job_db
- `MatchScores.JobId` references `Jobs.JobId` in job_db
- `Notifications.UserId` references a user ID from auth_db

Data consistency across services is maintained through events. For example, when a candidate submits an application, the ApplicationService fetches the job title from JobService via HTTP and stores it locally (denormalization) so it does not need to call JobService every time the application is displayed.

### Design Decisions

**UUIDs for primary keys.** All domain entities use UUID primary keys generated by PostgreSQL (`gen_random_uuid()`). This allows any service to generate IDs independently without coordination.

**Enums stored as strings.** Enums like JobStatus and ApplicationStatus are stored as varchar in the database rather than integers. This makes the data readable when querying directly and avoids issues when enum values are reordered.

**JSONB for nested data.** Complex structures like parsed resume data, match strengths, and skill gaps are stored as PostgreSQL JSONB columns. This keeps the schema flexible while still allowing indexed queries on JSON fields if needed.

**Denormalized fields.** The Applications table stores `CandidateName` and `JobTitle` locally even though they belong to other services. This avoids cross-service calls for common read operations.

---

## AI Agent System

The AI Service uses three specialized agents, each wrapping a Claude API call with a carefully tuned system prompt and structured JSON output parsing.

### ClaudeApiClient

The base HTTP client for all agent communication. Sends requests to the Anthropic Messages API and handles:

- Retry with exponential backoff for rate limits (HTTP 429) and overload responses (HTTP 529)
- Respect for `Retry-After` headers when present
- JSON extraction from responses (strips markdown code blocks if Claude wraps output)
- Self-correction: if the LLM returns invalid JSON, the client sends a follow-up message asking it to fix the output (up to 3 attempts)

### ResumeParserAgent

Takes raw text extracted from a PDF or DOCX and returns structured data:

```json
{
  "personalInfo": { "name": "", "email": "", "phone": "", "location": "" },
  "summary": "",
  "experience": [
    { "company": "", "title": "", "startDate": "", "endDate": "", "description": "", "highlights": [] }
  ],
  "education": [
    { "institution": "", "degree": "", "field": "", "graduationDate": "", "gpa": "" }
  ],
  "skills": [
    { "name": "", "category": "", "proficiencyLevel": "Beginner|Intermediate|Advanced|Expert", "yearsUsed": 0 }
  ],
  "certifications": [{ "name": "", "issuer": "", "date": "" }],
  "projects": [{ "name": "", "description": "", "technologies": [] }]
}
```

The parsing runs asynchronously through a `Channel<Guid>` queue consumed by `ResumeParsingWorker`. The flow is:

1. User uploads resume via POST /api/resumes
2. File is stored in AWS S3
3. Resume ID is pushed to the parsing channel
4. Background worker picks it up, extracts text (PdfPig for PDF, OpenXml for DOCX)
5. Text is sent to Claude via ResumeParserAgent
6. Structured result is saved to `ai_db.Resumes.ParsedData` and skills are extracted to `CandidateSkills`

### JobMatcherAgent

Computes a match score (0 to 100) between a candidate and a job posting. The prompt instructs Claude to evaluate:

- Skill alignment (exact matches and transferable skills)
- Years and type of experience
- Education fit
- Industry relevance and seniority level

The response includes an overall score, sub-scores for skills/experience/education, a plain-language explanation, and lists of strengths and gaps.

Match scores are cached in Redis to avoid redundant Claude API calls when the same candidate-job pair is viewed multiple times.

### SkillGapAnalyzerAgent

Given a candidate's skills and a target job, identifies what is missing and recommends a learning path. Returns missing skills ranked by priority, estimated time to readiness, and specific learning actions.

### Auto-Matching Workflow

When an employer publishes a job, the system triggers automatic matching:

1. JobService emits `JobPublishedEvent` to RabbitMQ
2. AIService's `JobMatchingWorker` consumes the event
3. The worker fetches all candidates with parsed resumes
4. For each candidate, it computes a match score via `JobMatcherAgent`
5. Results are stored in `MatchScores` and cached in Redis
6. High-scoring matches trigger notifications via RabbitMQ

---

## Event-Driven Communication

Services communicate asynchronously through RabbitMQ. The SharedKernel project defines the event contracts that all services share.

### Events

**JobPublishedEvent**
- Emitted by: JobService (when a job status changes to Active)
- Consumed by: AIService (triggers auto-matching for all candidates)
- Payload: JobId, EmployerId, Title, OccurredAt

**JobMatchedEvent**
- Emitted by: AIService (after computing a match score)
- Consumed by: NotificationService (notifies the candidate)
- Payload: JobId, JobTitle, CandidateId, MatchScore, OccurredAt

**ApplicationSubmittedEvent**
- Emitted by: ApplicationService
- Consumed by: NotificationService (notifies the employer)
- Payload: ApplicationId, CandidateId, JobId, OccurredAt

**ApplicationStatusChangedEvent**
- Emitted by: ApplicationService
- Consumed by: NotificationService (notifies the candidate)
- Payload: ApplicationId, CandidateId, JobId, OldStatus, NewStatus, OccurredAt

### Event Bus Implementation

The `RabbitMqEventBus` class in SharedKernel handles connection management, channel creation, and message serialization. Each service creates a singleton instance on startup. Events are serialized as JSON and published to topic exchanges.

---

## Authentication and Authorization

### JWT Strategy

AuthService generates JWT tokens on successful login. All other services validate these tokens using the same shared secret. The token contains standard claims plus user roles.

**Token configuration:**
- Issuer: configurable via `JWT_ISSUER` (default: "matchura")
- Audience: configurable via `JWT_AUDIENCE` (default: "matchura-clients")
- Signing: HMAC-SHA256 with a shared secret (`JWT_SECRET`)
- Expiry: 24 hours (configurable)

### Role-Based Access Control

Three roles are seeded on startup:
- **Candidate** - can browse jobs, upload resumes, submit applications, view match scores and skill gap reports
- **Employer** - can post jobs, review applications, add notes, view candidate match scores
- **Admin** - full access

Roles are stored in the `AspNetUserRoles` table and included in JWT claims. Controllers use `[Authorize(Roles = "...")]` attributes for endpoint-level access control.

### Authentication Flow

1. User registers via POST /api/auth/register
2. A 6-digit verification code is emailed
3. User verifies email via POST /api/auth/verify-email
4. User logs in via POST /api/auth/login
5. If 2FA is enabled, a second code is emailed and verified
6. JWT token is returned on success
7. Frontend stores the token and includes it as `Authorization: Bearer <token>` on subsequent requests

### Gateway Validation

The API Gateway validates JWT tokens before forwarding requests to downstream services. This means services behind the gateway can trust that authenticated requests have already been verified. The gateway also passes the token through so services can extract user claims (like user ID and roles) from it.

---

## API Gateway

The gateway is built with YARP (Yet Another Reverse Proxy) and serves as the single entry point for all client traffic.

### Route Configuration

| Pattern | Target Service | Rate Limit Tier |
|---------|---------------|-----------------|
| /api/auth/* | AuthService | Anonymous (30/min) |
| /api/profiles/* | ProfileService | Authenticated (100/min) |
| /api/jobs/*, /api/skills/* | JobService | Authenticated (100/min) |
| /api/applications/* | ApplicationService | Authenticated (100/min) |
| /api/resumes/*, /api/documents/* | AIService | Authenticated (100/min) |
| /api/matching/*, /api/skillgap/* | AIService | AI (20/min) |
| /api/analytics/* | AIService | Authenticated (100/min) |
| /api/notifications/* | NotificationService | Authenticated (100/min) |
| /notifications-hub/* | NotificationService | Authenticated (100/min) |

### Rate Limiting

Three tiers using ASP.NET Core's built-in rate limiter with fixed windows:

- **Anonymous (30 req/min):** Applied to auth endpoints. Prevents brute-force login attempts.
- **Authenticated (100 req/min):** Standard limit for logged-in users.
- **AI (20 req/min):** Applied to matching and skill gap endpoints. These trigger Claude API calls which are expensive and slow.

### Health Checks

The gateway monitors all downstream services by polling their `/health` endpoints. The gateway's own `/health` endpoint aggregates the results so you can check overall system health with a single call.

---

## Frontend Application

The frontend is a Next.js 16 app using the App Router with React 19. It uses Zustand for client-side state management, React Hook Form with Zod for form validation, and a SignalR client for real-time notifications.

### Page Structure

**Public routes:**
- `/login` - Authentication
- `/register` - User registration with role selection
- `/verify-email` - Email confirmation
- `/jobs` - Job browsing and search
- `/jobs/[id]` - Job detail with match score (if logged in as candidate)

**Candidate routes (protected):**
- `/dashboard` - Overview with recent applications and match scores
- `/applications` - List of submitted applications
- `/applications/[id]` - Application detail with status tracking
- `/resumes` - Resume upload and management
- `/skill-gap` - AI-generated skill gap analysis

**Employer routes (protected):**
- `/employer/dashboard` - Overview with job posting stats
- `/employer/jobs/create` - Create new job posting
- `/employer/jobs/[id]/applicants` - View and rank applicants
- `/employer/jobs/[id]/applicants/[applicationId]` - Review individual application with match score, inline PDF viewing, and notes
- `/employer/analytics` - Hiring analytics with Recharts visualizations

### Route Protection

A Next.js middleware checks for a JWT token (in cookies or Authorization header) on protected routes. Unauthenticated users are redirected to `/login` with a redirect parameter so they return to the original page after logging in.

### Real-Time Notifications

The frontend connects to the NotificationService's SignalR hub at `/notifications-hub` using the `@microsoft/signalr` package. The connection is authenticated using the JWT token passed as a query parameter. When the server pushes a notification, the UI updates immediately without polling.

### Component Architecture

```
components/
  ui/          Reusable atoms: buttons, cards, modals, inputs
  layout/      Page layouts, navbar, sidebar
  composed/    Medium-complexity components combining UI atoms
  features/    Feature-specific components (job cards, resume viewer, match display)
```

---

## Testing Strategy

### Backend Unit Tests

Every service has a corresponding unit test project using xUnit:
- AuthService.UnitTests
- ProfileService.UnitTests
- JobService.UnitTests
- ApplicationService.UnitTests
- AIService.UnitTests
- NotificationService.UnitTests

Shared test utilities (fixtures, mocks, helpers) live in `Shared.TestUtilities`.

### Backend Integration Tests

Three services have integration test projects that test full request/response cycles against real PostgreSQL and RabbitMQ instances:
- JobService.IntegrationTests
- ApplicationService.IntegrationTests
- AIService.IntegrationTests

### Frontend Unit Tests

Component and utility tests run with Vitest and @testing-library/react.

### End-to-End Tests

Playwright tests in `src/frontend/web/e2e/` cover full user flows (registration, job posting, application submission). Test helpers handle user registration, email verification, and job seeding via direct API calls.

---

## CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/deploy.yml`) runs on every push to `main`:

### Stage 1: Backend Tests
- Sets up .NET 10 SDK
- Restores dependencies and builds all services
- Runs unit tests for all 6 services
- Runs integration tests for JobService and ApplicationService

### Stage 2: Frontend Tests
- Sets up Node.js 22
- Installs dependencies with `npm ci`
- Runs Vitest

### Stage 3: Deploy (depends on both test stages passing)
- Installs Railway CLI
- Deploys each service individually with `railway up --service <name> --detach`
- Services deployed: auth, profile, job, application, ai, notification, gateway, frontend

### Stage 4: Sentry Release
- Creates a release in Sentry for both `matchura-services` and `matchura-frontend` projects
- Tags the release with the production environment

---

## Containerization

### Backend Services

All .NET services use the same two-stage Dockerfile pattern:

**Build stage** (mcr.microsoft.com/dotnet/sdk:10.0):
1. Copy SharedKernel.csproj and the service .csproj
2. `dotnet restore` to cache NuGet packages
3. Copy full source
4. `dotnet publish -c Release -o /app/publish --no-restore`

**Runtime stage** (mcr.microsoft.com/dotnet/aspnet:10.0):
1. Copy published binaries from build stage
2. Expose port 8080
3. Set `ASPNETCORE_HTTP_PORTS=8080`
4. `ENTRYPOINT ["dotnet", "<ServiceName>.dll"]`

### Frontend

Three-stage Dockerfile:

**Deps stage** (node:20-alpine):
1. Copy package.json and package-lock.json
2. `npm ci`

**Builder stage** (node:20-alpine):
1. Copy node_modules from deps
2. Accept `NEXT_PUBLIC_API_URL` as build arg
3. `npm run build`

**Runtime stage** (node:20-alpine):
1. Copy node_modules, .next/, public/, package.json
2. `NODE_ENV=production`
3. Expose port 3000
4. `CMD ["npx", "next", "start"]`

### Docker Compose

The `docker-compose.yml` defines the full local development stack:
- PostgreSQL 16 with automatic creation of 6 databases via `init-databases.sh`
- Redis 7 with persistent volume
- RabbitMQ 4 with management UI on port 15672
- pgAdmin on port 5050
- All 7 backend services and the frontend
- Health checks and dependency ordering so services start in the right sequence

All credentials are read from environment variables (the `.env` file, which is gitignored).

---

## Monitoring and Observability

### Sentry Integration

Every service initializes Sentry on startup through a shared `AddMatchuraSentry()` extension method in SharedKernel. Configuration includes:

- **Distributed tracing:** Requests are traced across service boundaries
- **Trace sample rates:** 100% in development, 20% in production
- **Profile sample rates:** 100% in development, 10% in production
- **Auto session tracking:** Enabled
- **PII:** Not sent (`SendDefaultPii = false`)
- **Default tags:** Each service is tagged with its name for filtering

The frontend uses `@sentry/nextjs` with:
- Client-side browser tracing and session replay
- Server-side request error capture with route metadata
- A `/monitoring` tunnel route that proxies Sentry requests through the server (avoids ad blockers)

### Request Logging

The API Gateway logs every request with method, path, status code, and duration in milliseconds. This runs as inline middleware before the reverse proxy.

---

## Deployment Instructions

### Local Development

1. Clone the repository:
   ```bash
   git clone https://github.com/Arobce/matchura.git
   cd matchura
   ```

2. Create your environment file:
   ```bash
   cp .env.example .env
   ```

3. Fill in the required values in `.env`:
   - `POSTGRES_USER` and `POSTGRES_PASSWORD` for the local database
   - `JWT_SECRET` (generate a random string, at least 32 characters)
   - `ANTHROPIC_API_KEY` from https://console.anthropic.com
   - `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY` for S3 resume storage
   - `RABBITMQ_USER` and `RABBITMQ_PASS`

4. Start everything:
   ```bash
   docker-compose up --build
   ```

5. Verify all services are healthy:
   ```bash
   for port in 5001 5002 5003 5004 5005 5006 5010; do
     echo "Port $port: $(curl -s http://localhost:$port/health)"
   done
   ```

6. The frontend is available at http://localhost:3001

### Running Individual Services

To work on a single service without Docker:

```bash
# Start infrastructure only
docker-compose up postgres redis rabbitmq

# Run the service locally
dotnet run --project src/services/AuthService
```

### Running Tests

```bash
# All backend tests
dotnet test

# Specific test project
dotnet test tests/JobService.UnitTests

# Frontend tests
cd src/frontend/web
npx vitest run

# E2E tests (requires running stack)
cd src/frontend/web
npx playwright test
```

### Production Deployment (Railway)

The project deploys to Railway through GitHub Actions. On every push to `main`, the pipeline runs tests and then deploys each service using the Railway CLI.

**Prerequisites:**
- A Railway account with a project set up
- `RAILWAY_TOKEN` stored as a GitHub repository secret
- Environment variables configured in Railway for each service

**Manual deployment:**
```bash
# Install Railway CLI
npm install -g @railway/cli

# Login
railway login

# Deploy a specific service
railway up --service auth-service --detach
```

Each Railway service needs the same environment variables listed in `.env.example`, but with production values (production database URLs, real API keys, etc.).

### Database Migrations

All services run Entity Framework Core migrations automatically on startup (`db.Database.MigrateAsync()`). There is no separate migration step needed during deployment. When a service starts with a new migration, it applies it before accepting traffic.

To create a new migration during development:

```bash
cd src/services/JobService
dotnet ef migrations add MigrationName
```
