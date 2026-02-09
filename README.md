# PetWorld

Minimal recruitment-task implementation of a pet e-commerce assistant built with **.NET 8**, **Blazor Server**, **MySQL**, and a **Writer–Critic** flow.

## Features

- Clean/Onion architecture with 4 projects:
  - `PetWorld.Domain`
  - `PetWorld.Application`
  - `PetWorld.Infrastructure`
  - `PetWorld.Web`
- Chat page (`/chat`) to submit a question and receive an answer
- History page (`/history`) with recent saved conversations
- Persistent storage in MySQL via EF Core
- Automatic database migration on application startup
- Writer–Critic loop with max 3 iterations
- AI configuration via environment variables
- Safe fallback mode (stub answers) when AI credentials are missing

---

## Tech Stack

- .NET 8
- ASP.NET Core Blazor Web App (Server interactivity)
- Entity Framework Core 8
- Pomelo.EntityFrameworkCore.MySql
- MySQL 8
- Docker Compose

---

## Project Structure

```text
PetWorld/
├─ src/
│  ├─ PetWorld.Domain/
│  ├─ PetWorld.Application/
│  ├─ PetWorld.Infrastructure/
│  └─ PetWorld.Web/
├─ docker-compose.yml
└─ README.md
```

## Layer Responsibilities

- **Domain**: domain models and business-oriented contracts/constants
- **Application**: use-case interfaces (e.g., AI workflow abstractions)
- **Infrastructure**: EF Core persistence, repositories/stores, AI service implementations
- **Web**: Blazor UI, DI composition, startup pipeline

---

## Prerequisites

- Docker Desktop installed and running
- (Optional) .NET SDK 8 for local build outside Docker

---

## Quick Start

From the repository root:

```bash
docker compose up --build
```

Open in browser:

- `http://localhost:5000/chat`
- `http://localhost:5000/history`

To stop:

```bash
docker compose down
```

To reset containers and volumes:

```bash
docker compose down -v
```

---

## Environment Variables (AI)

The app supports AI configuration from ENV variables.

Create a local `.env` file in repository root (do **not** commit it):

```env
AZURE_OPENAI_ENDPOINT=
AZURE_OPENAI_KEY=
AZURE_OPENAI_DEPLOYMENT=gpt-4o-mini
```

### Behavior without credentials

If AI credentials are missing, the app remains fully functional and returns **stub answers** (fallback mode).  
This is intentional to keep setup friction low and ensure the app runs in any environment.

---

## Database

- Database: MySQL (`petworld`)
- Main table: `ChatMessages`:
  - `Id`
  - `CreatedAtUtc`
  - `Question`
  - `Answer`
  - `Iterations`

### Migrations

Migrations are applied automatically on app startup via `Database.Migrate()`.  
No manual `dotnet ef database update` is required for normal Docker flow.

---

## Useful Commands

Build solution locally:

```bash
dotnet build PetWorld.sln -v:minimal
```

Check recent messages in DB:

```bash
docker compose exec db mysql -upetworld -ppetworld -D petworld -e "SELECT Id, CreatedAtUtc, LEFT(Question,80) AS Q, LEFT(Answer,80) AS A, Iterations FROM ChatMessages ORDER BY Id DESC LIMIT 10;"
```

Check tables:

```bash
docker compose exec db mysql -upetworld -ppetworld -D petworld -e "SHOW TABLES;"
```

---

## Minimal Smoke Test

1. Run `docker compose up --build`
2. Open `/chat`
3. Submit a question
4. Verify answer + iterations are visible
5. Open `/history` and confirm the record appears
6. (Optional) verify rows via SQL command above

---

## Troubleshooting

### `dotnet` not recognized on Windows

Install SDK and restart terminal:

```powershell
winget install Microsoft.DotNet.SDK.8
```

Then verify:

```bash
dotnet --version
```

### PowerShell parsing errors with JSON (`Unexpected token ':'`)

You pasted raw JSON into terminal as commands.  
Use `notepad` to edit JSON files or write them with here-strings.

### Blazor button click does nothing

Ensure interactive server mode is enabled:

- `AddInteractiveServerComponents()`
- `AddInteractiveServerRenderMode()`
- `@rendermode InteractiveServer` on interactive pages

### You still see `Stub answer`

AI ENV variables are missing/invalid, or not passed into the `web` container.

---

## Security Notes

- Never commit real secrets (keys/tokens)
- Keep `.env` in `.gitignore`
- Commit only `.env.example`

---

## Roadmap / Next Steps

- Full Microsoft Agent Framework orchestration for Writer and Critic agents
- Externalized product catalog (JSON/DB) instead of inline string
- Better validation and error handling
- Integration tests for chat/history workflows
- Improved UI/UX polish

---

## Author

Mariusz
