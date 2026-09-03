# PlayBoard

A Wordle-style word-guessing game built as an ASP.NET Core 8 Web API, with JWT authentication, role-based admin access, and a vanilla JS/HTML frontend. Deployed to Azure App Service via GitHub Actions.

**Live app:** https://playboard-gwdzehg5bee5fje4.indiasouthcentral-01.azurewebsites.net

## Features

- **Word-guessing gameplay** — server generates a random word, tracks it per authenticated user, and returns a Wordle-style green/yellow/gray comparison for each guess.
- **JWT authentication** — register and log in via hashed credentials (`Microsoft.AspNetCore.Identity.PasswordHasher`), receive a bearer token for subsequent requests.
- **Role-based admin access** — a configured allowlist of admin usernames receive an `Admin` role claim on login, enforced by both a custom authorization middleware (perimeter check on `/api/Admin/*`) and policy-based `[Authorize]` attributes (per-endpoint check).
- **Admin endpoints** — list all registered usernames and the full word bank.
- **Global exception handling** — unhandled exceptions are caught centrally via `IExceptionHandler`, logged server-side with a trace ID, and returned to clients as a generic `ProblemDetails` response (no internal details leaked in production).
- **Frontend game UI** — login/sign-up, live tile-based guess input, persistent letter-reveal pattern, guess history, and a reveal option after repeated failed attempts.

## Tech stack

- **.NET 8** / ASP.NET Core Web API
- **JWT Bearer authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Swagger / OpenAPI** (Swashbuckle) for API exploration
- Plain **JSON file storage** for users and words (see [Known limitations](#known-limitations))
- **Vanilla HTML/CSS/JS** frontend (no framework), served from `wwwroot`
- **GitHub Actions** → **Azure App Service** for CI/CD

## Project structure

```
PlayBoard/
├── ClassCollection/       GuessTheWord.cs (game logic), Auth.cs (authorization policy classes)
├── Controllers/           AuthController, GuessTheWordController, AdminController
├── DataCollection/        UserData.json, WordCollection.js — flat-file data store
├── Middleware/             AdminAccessMiddleware, ExceptionHandlingMiddleware, GlobalExceptionHandler
├── ModelCollection/       Request/response DTOs and view models
├── Services/               Auth/user-store interfaces and implementations, in-memory game state
├── wwwroot/                Frontend HTML/CSS/JS
├── Program.cs
└── appsettings.json
```

## API endpoints

| Method | Route | Auth required | Description |
|---|---|---|---|
| POST | `/api/Auth/Register` | No | Create a new user account |
| POST | `/api/Auth/Login` | No | Authenticate and receive a JWT |
| GET | `/api/GuessTheWord/GetNewWord` | Yes | Get a new random word (stored server-side against the caller) |
| GET | `/api/GuessTheWord/GetComparision?guess=` | Yes | Compare a guess against the caller's active word |
| GET | `/api/Admin/Users` | Yes (Admin role) | List all registered usernames |
| GET | `/api/Admin/Words` | Yes (Admin role) | List the full word bank |

## Running locally

```bash
git clone https://github.com/SanjivKhareGitSync/PlayBoard.git
cd PlayBoard
dotnet restore
dotnet user-secrets set "Jwt:Key" "<a-strong-dev-only-secret>"
dotnet run
```

Open `wwwroot/GuessTheWord.html` (or the equivalent dev-pointed copy) in a browser, or hit the API directly via the built-in Swagger UI at `/swagger`.

### Configuration

| Setting | Where | Notes |
|---|---|---|
| `Jwt:Key` / `Jwt:Issuer` / `Jwt:Audience` / `Jwt:ExpiryMinutes` | `appsettings.json` (non-secret parts) + environment variable / user-secrets (`Key`) | Signing key must never be committed |
| `AdminUsers` | `appsettings.json` or `AdminUsers__0`, `AdminUsers__1`, ... env vars | Usernames granted the `Admin` role on login |

## Deployment

Every push to `master` triggers a GitHub Actions workflow (`.github/workflows/master_playboard.yml`) that builds, publishes, and deploys straight to the Azure App Service `PlayBoard` (Production slot) using a publish-profile secret.

## Known limitations

This is an active learning project — some simplifications are deliberate for now, not oversights:

- **File-based storage** — users and words live in flat JSON files rather than a database. An `IUserStore` abstraction already exists specifically so this can be swapped for a SQL-backed implementation without touching business logic.
- **In-memory game state** — the word currently assigned to each user is held in server memory, not persisted. It resets if the app restarts, and would need a shared store (e.g. Redis or the same future database) before running on more than one instance.
- **Swagger enabled in Production** — intentionally left on for now to aid learning/testing; would be disabled or access-restricted in a hardened deployment.

## Roadmap

- Migrate `IUserStore` to a SQL-backed implementation (Azure SQL)
- One-time payment integration (Razorpay) for unlockable features
- Folder/namespace restructuring (`Services` → `DataAccess` / `Authorization` split, `Models`/`Data` renames)
