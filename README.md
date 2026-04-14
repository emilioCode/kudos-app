# 🎉 Kudos App

**Peer-to-peer employee recognition platform** that empowers teams to celebrate contributions, earn points, and unlock achievements.
Built as a full-stack technical showcase featuring modern web technologies, clean architecture, and AI-powered features.

---

## 📖 Overview

Kudos App is an employee recognition platform where team members can:

- **Give kudos** to colleagues with personalized messages and categories
- **Earn points** for receiving kudos based on recognition type
- **Unlock badges** automatically when reaching point thresholds
- **Compete on leaderboards** with top contributors earning medals (🥇🥈🥉)
- **View a public feed** of all kudos shared across the organization
- **Get AI assistance** for writing impactful kudos messages

---

## ✨ Features

### Core Functionality
- ✅ **User registration and login** with JWT authentication
- ✅ **Give kudos** with recipient selection, category, and custom message
- ✅ **Public kudos feed** displaying all recognition in real-time
- ✅ **Points system** — each category awards different point values
- ✅ **Badges system** — automatically awarded when users reach point thresholds
- ✅ **Leaderboard** with top 3 users earning medals (🥇🥈🥉)
- ✅ **Role-based access** (User and Admin roles)

### Admin Features
- 📊 **Admin dashboard** with analytics and insights
- 👥 **User management** — view, edit, and manage all users
- 📈 **Recognition metrics** — track kudos trends and engagement

### AI-Powered Features
- 🤖 **Kudos Writer Assistant** — AI suggests personalized kudos messages based on recipient and category
- 🏷️ **Auto-categorization** — AI analyzes message content and suggests the most appropriate category

---

## 🛠️ Tech Stack

### Backend
- **.NET 9** — Latest performance improvements and minimal APIs
- **MediatR (CQRS)** — Clean separation of Commands and Queries, better maintainability
- **Entity Framework Core** — Code-first approach with migrations
- **SQL Server** — Relational database, production-ready and scalable
- **JWT Authentication** — Stateless, secure token-based auth
- **FluentValidation** — Input validation with expressive, readable rules

### Frontend
- **Angular 21** — Standalone components, modern reactive architecture
- **Signals** — Angular's new reactivity primitive for fine-grained updates
- **PrimeNG** — Enterprise-grade UI component library
- **Reactive Forms** — Type-safe, reactive form handling
- **OnPush Change Detection** — Optimized performance strategy
- **TypeScript** — Fully typed, no `any` usage

### Testing
- **xUnit** — .NET test framework
- **Moq** — Mocking dependencies
- **FluentAssertions** — Readable, expressive assertions
- **InMemory Database** — Fast, isolated integration tests

### AI Integration
- **OpenAI GPT-4o-mini** — Powers both AI features via raw HTTP calls (no SDK)

---

## 🤖 AI Tools Used

### GitHub Copilot
- **Code scaffolding** — Generated MediatR handlers, Angular components, and test boilerplate
- **Test generation** — Created comprehensive unit tests with xUnit and FluentAssertions
- **Prompt engineering** — Assisted in crafting OpenAI prompts for AI features
- **Custom instructions** — Project-specific guidelines in `.github/copilot-instructions.md`

### OpenAI GPT-4o-mini
- **Kudos Writer Assistant** — Generates contextual kudos messages
- **Auto-categorization** — Intelligently categorizes kudos based on content
- **Integration** — Direct HTTP REST calls from `KudosApp.Infrastructure/Services/OpenAiService.cs`

---

## 🚀 Setup and Run

### Prerequisites
- **.NET 9 SDK** — [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+** — [Download here](https://nodejs.org/)
- **SQL Server** — Local instance or connection string to remote server

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/{username}-applaudo/kudos-app.git
cd kudos-app
```

2. **Configure the backend**
```bash
cd backend/KudosApp.Api
cp appsettings.example.json appsettings.Development.json
```

Edit `appsettings.Development.json` with your values:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=KudosApp;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "YOUR_JWT_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "kudos-app",
    "Audience": "kudos-app-users"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY",
    "Model": "gpt-4o-mini"
  }
}
```

3. **Run the backend**
```bash
cd backend
dotnet run --project KudosApp.Api
```
Backend runs on `https://localhost:7162`

4. **Run the frontend**
```bash
cd frontend/kudos-angular
npm install
ng serve
```
Frontend runs on `http://localhost:4200`

5. **Run the tests**
```bash
cd backend
dotnet test
```

---

## 🤖 AI Features Implemented

### Kudos Writer Assistant
- **Endpoint:** `POST /api/ai/suggest-message`
- **Input:** recipient name + category name
- **Output:** personalized kudos message
- **How to use:** In the Give Kudos form, select a recipient and category then click "Suggest Message"

### Auto-categorization
- **Endpoint:** `POST /api/ai/suggest-category`
- **Input:** free-text message + list of available categories
- **Output:** best matching category name
- **How to use:** Write your message freely then click "Auto-detect Category"

Both features use **OpenAI GPT-4o-mini** via raw REST calls in `KudosApp.Infrastructure/Services/OpenAiService.cs`

---

## 🔮 What I Would Do Next

- **Docker Compose** — single command setup with `docker compose up`
- **SignalR** — real-time kudos feed without page refresh
- **Email notifications** — notify users when they receive kudos
- **Slack/Teams integration** — share kudos directly to team channels
- **Gift catalog** — point redemption system with rewards
- **Angular unit tests** — component and service testing with Jasmine
- **CI/CD pipeline** — automated testing and deployment with GitHub Actions
- **Rate limiting** — protect AI endpoints from abuse

---

## 📁 Project Structure

```
kudos-app/
├── .github/
│   └── copilot-instructions.md
├── backend/
│   ├── KudosApp.Api/            # Controllers, Program.cs
│   ├── KudosApp.Application/    # Commands, Queries, DTOs, Interfaces
│   ├── KudosApp.Domain/         # Entities, Enums
│   ├── KudosApp.Infrastructure/ # Handlers, DbContext, Services
│   └── KudosApp.Tests/          # xUnit tests
└── frontend/
    └── kudos-angular/           # Angular 21 standalone app
```