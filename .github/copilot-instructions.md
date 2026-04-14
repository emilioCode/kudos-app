 
# Kudos App — Copilot Instructions

## Project Overview
Peer-to-peer employee recognition app. Employees give kudos, earn points, and celebrate contributions.
Built as a technical assessment showcasing full-stack development with AI features.

## Backend (.NET 8)
- Web API with Controllers
- MediatR for CQRS — Commands and Queries strictly separated
- Entity Framework Core with SQL Server
- JWT authentication (access token in Authorization header)
- FluentValidation for all input validation
- Never put business logic in Controllers — always delegate to MediatR handlers
- Folder structure: Controllers / Application / Domain / Infrastructure

## Frontend (Angular)
- Angular 17+ with standalone components only — no NgModules
- PrimeNG for all UI components
- Reactive Forms only — never template-driven
- HttpClient with interceptors for JWT token injection
- Feature-based folder structure: features/auth, features/kudos, features/admin
- OnPush change detection strategy
- No use of `any` type — always typed

## AI Features (OpenAI)
- Kudos Writer Assistant: given recipient + category, suggest a kudos message
- Auto-categorization: given a free-text message, suggest the best category
- OpenAI calls via raw HttpClient REST — no SDK
- AI service: Infrastructure/AI/OpenAiService.cs
- Stream responses where possible for better UX

## Database (SQL Server)
- Managed via EF Core migrations
- Main entities: Users, Kudos, Categories, Badges, UserBadges
- Seed data for Categories and demo users on startup

## Running the project
- Single command: `docker compose up`
- Backend: http://localhost:5000
- Frontend: http://localhost:4200

## Code conventions
- C#: async/await everywhere, no sync-over-async, PascalCase
- Commits: conventional commits — feat:, fix:, chore:, refactor:
- Never commit secrets — use .env or appsettings.Development.json
