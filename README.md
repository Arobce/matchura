# Matchura — Intelligent Job Matching Platform

A microservices-based job matching platform with AI-powered resume screening, candidate-job matching, and skill gap analysis.

## Stack

- **Backend:** ASP.NET Core 10 (microservices)
- **Frontend:** Next.js 14 with TypeScript, Tailwind CSS
- **Database:** PostgreSQL 16 (database-per-service)
- **Cache:** Redis 7
- **AI:** Claude API for resume parsing, matching, and skill gap analysis
- **Infrastructure:** Docker, Kubernetes

## Architecture

```
┌──────────────┐
│   Frontend   │  Next.js 14
│  (port 3000) │
└──────┬───────┘
       │
┌──────▼───────┐
│  API Gateway │  YARP reverse proxy
│  (port 5000) │
└──────┬───────┘
       │
┌──────▼────────────────────────────────────────┐
│                Microservices                   │
├────────────┬────────────┬────────────┬────────┤
│ Auth       │ Profile    │ Job        │ App    │
│ (5001)     │ (5002)     │ (5003)     │ (5004) │
├────────────┴────────────┴────────────┴────────┤
│ AI Service (5005)                              │
│ Resume parsing · Job matching · Skill gap      │
└────────────────────────────────────────────────┘
       │                    │
┌──────▼───────┐    ┌──────▼───────┐
│  PostgreSQL  │    │    Redis     │
│  (5 DBs)     │    │   (cache)    │
└──────────────┘    └──────────────┘
```

## Quick Start

### Prerequisites

- Docker and Docker Compose
- .NET 10 SDK (for local development)
- Node.js 20+ (for frontend development)

### Run everything

```bash
docker-compose up --build
```

### Verify services are running

```bash
curl http://localhost:5001/health  # Auth
curl http://localhost:5002/health  # Profile
curl http://localhost:5003/health  # Job
curl http://localhost:5004/health  # Application
curl http://localhost:5005/health  # AI
curl http://localhost:5000/health  # Gateway
```

### Database management

pgAdmin is available at `http://localhost:5050`
- Email: `admin@matchura.dev`
- Password: `admin`

## Project Structure

```
matchura/
├── docker-compose.yml
├── .env
├── src/
│   ├── services/
│   │   ├── AuthService/           # Authentication and user management
│   │   ├── ProfileService/        # Candidate and employer profiles
│   │   ├── JobService/            # Job posting and search
│   │   ├── ApplicationService/    # Application pipeline
│   │   ├── AIService/             # Resume parsing, matching, skill gap
│   │   └── ApiGateway/            # YARP reverse proxy
│   ├── shared/
│   │   └── SharedKernel/          # Shared DTOs, interfaces, utilities
│   └── frontend/
│       └── web/                   # Next.js app
├── k8s/                           # Kubernetes manifests
├── scripts/                       # Helper scripts
└── tests/                         # Test projects
```

## Development

### Build just one service

```bash
docker-compose up --build auth-service
```

### Run .NET services locally

```bash
dotnet run --project src/services/AuthService
```

### Run all tests

```bash
dotnet test
```
