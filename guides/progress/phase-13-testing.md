# Phase 13: Comprehensive Testing Suite

## Overview

Added a full testing suite covering all 6 backend microservices and the Next.js frontend. The project now has **479 tests total** (349 backend + 130 frontend) across unit, integration, validator, contract, and component test layers.

## What Was Built

### Backend Test Infrastructure
- **10 test projects** (6 unit, 3 integration, 1 shared utilities)
- **Shared.TestUtilities** library with:
  - `FakeEventBus`, `FakeEmailService`, `FakeCacheService`, `FakeS3StorageService`
  - `MockHttpMessageHandler` for inter-service HTTP mocking
  - `DbContextFactory` for SQLite in-memory databases
  - `FakeClaudeHandler` for mocking Claude API responses
  - `CustomWebApplicationFactory` with Testcontainers PostgreSQL
  - JSON fixture files for snapshot testing
- All projects target .NET 10 with xUnit, Moq, FluentAssertions

### Unit Tests (197 tests)
| Service | Tests | Key Coverage |
|---------|-------|-------------|
| ApplicationService | 43 | State machine (6 valid + 11 invalid transitions), CRUD, event publishing, authorization |
| JobService | 29 | Status transitions, filtering, skills, event publishing, ownership |
| AuthService | 27 | Registration, email verification, 2FA, login flows, JWT |
| NotificationService | 25 | Create with SignalR push, pagination, mark read |
| ProfileService | 18 | Candidate/employer CRUD, public vs private views |
| AIService | 10 | Matching cache hit/miss, resume upload with S3 |

### AI Agent Snapshot Tests (29 tests)
- **ResumeParserAgent** (7 tests) — fixture-based parsing validation, proficiency levels, error handling
- **JobMatcherAgent** (10 tests) — score validation, explanation quality, error scenarios
- **SkillGapAnalyzerAgent** (12 tests) — readiness scores, missing skills, recommended actions

### Background Worker Tests (17 tests)
- **ResumeParsingWorker** (6 tests) — parse success/failure, skill population
- **RabbitMqConsumerWorker** (11 tests) — all 4 event handlers, notification creation

### Validator Tests (93 tests)
- **AuthService** (52 tests) — email, password, role, 2FA code validation
- **JobService** (30 tests) — title, salary, experience, deadline validation
- **ApplicationService** (11 tests) — job ID, cover letter, URL limits

### Event Contract Tests (31 tests)
- JSON serialization round-trips for all 5 SharedKernel events
- camelCase property name verification
- Property completeness assertions

### Integration Tests (28 tests)
- **JobService** (12 tests) — full lifecycle with real PostgreSQL via Testcontainers
- **ApplicationService** (16 tests) — submit/review/accept pipeline with real DB
- Uses `CustomWebApplicationFactory` with auto-migration and JWT token generation

### Frontend Tests (130 tests)
| Category | Tests | Coverage |
|----------|-------|----------|
| UI Components | 53 | Button, Input, Badge, Modal, Pagination, Alert |
| Composed Components | 25 | StatusBadge, SkillBadge, ScoreDisplay |
| Feature Components | 8 | JobCard |
| Zustand Stores | 27 | jobFilterStore, notificationStore, uiStore |
| Utility Functions | 15 | auth token handling |

**Frontend Stack:** Vitest + React Testing Library + MSW v2

## Key Decisions

### Updated Test Plan vs Original
The original test plan (`guides/testing.MD`) had several inaccuracies:
- Referenced .NET 8 (now .NET 10)
- Used wrong application statuses (Applied/Screening/Interviewed vs Submitted/Reviewed/Shortlisted)
- Assumed IJobRepository/IMapper (services use DbContext directly)
- Missing coverage for events, workers, notifications, 2FA

### Actual Application Status Transitions
```
Submitted -> Reviewed
Reviewed  -> Shortlisted | Rejected
Shortlisted -> Accepted | Rejected
Submitted -> Rejected
```
Terminal states: Accepted, Rejected, Withdrawn

### Testing Patterns Used
- **SQLite in-memory** for unit tests (with SavingChanges hook for GUID generation)
- **Testcontainers PostgreSQL** for integration tests (real DB behavior)
- **FakeEventBus** pattern across 3 services for event verification
- **InternalsVisibleTo** for worker testability
- **MSW v2** for frontend API mocking
- **Snapshot testing** with fixture files for LLM agent responses

## Running Tests

```bash
# Backend (all 349 tests)
dotnet test

# Frontend (all 130 tests)
cd src/frontend/web && npx vitest run

# Watch mode (frontend)
cd src/frontend/web && npx vitest
```

## Files Modified

### Source changes for testability
- `src/services/AIService/AIService.csproj` — InternalsVisibleTo
- `src/services/NotificationService/NotificationService.csproj` — InternalsVisibleTo
- `src/services/AIService/Agents/JobMatcherAgent.cs` — made ComputeMatchAsync virtual
- `src/services/AIService/Agents/ResumeParserAgent.cs` — made ParseAsync virtual
- `src/services/AIService/Infrastructure/BackgroundJobs/ResumeParsingWorker.cs` — ProcessResumeAsync internal
- `src/services/NotificationService/Infrastructure/Services/RabbitMqConsumerWorker.cs` — handler methods internal
- `src/services/JobService/Program.cs` — public partial class Program
- `src/services/ApplicationService/Program.cs` — public partial class Program

### Test projects added
```
tests/
├── Shared.TestUtilities/          (fakes, fixtures, DbContextFactory)
├── AuthService.UnitTests/         (27 tests)
├── ProfileService.UnitTests/      (18 tests)
├── JobService.UnitTests/          (90 tests)
├── ApplicationService.UnitTests/  (54 tests)
├── AIService.UnitTests/           (44 tests)
├── NotificationService.UnitTests/ (36 tests)
├── JobService.IntegrationTests/   (12 tests)
├── ApplicationService.IntegrationTests/ (16 tests)
└── AIService.IntegrationTests/    (empty, ready for future)
```
