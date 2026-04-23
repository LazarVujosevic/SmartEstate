# SmartEstate — Shared Project Knowledge Base

> Every session (Lead Dev, Backend Dev, Frontend Dev) **must read this file in full before starting any work**.
> When working on a sprint, also read **`.claude/sprint-plans.md`** — it contains issue trackers, API contracts, and implementation notes for every sprint.
> Update this file when making architectural decisions, discovering constraints, or completing sprints.

---

## Project Overview

SmartEstate is a **multi-tenant SaaS platform** for real estate agencies. It helps agents find the right buyers for properties and alerts agencies about FSBO (For Sale By Owner) listings before competitors do.

**Owner / Product Owner:** Lazar Vujosevic (lazar.vujosevic100@gmail.com)  
**Repository:** https://github.com/LazarVujosevic/SmartEstate  
**Communication language with owner:** Serbian  
**All .md files and code comments:** English

---

## Core Features

### Phase 1 — Lifestyle Matcher
- Agents enter buyer profiles with free-text lifestyle preferences
- AI (Gemini) analyzes descriptions and assigns semantic tags + structured profile
- Agents enter property listings with descriptions and images
- System matches buyers to properties based on tag/profile similarity
- Agents log buyer reactions; AI refines matching quality based on feedback

### Phase 2 — Lead-Gen (FSBO Detection)
- Background workers scrape real estate portals for new listings
- AI classifies listings as agency-posted vs. private-seller (FSBO)
- Active agencies are immediately notified of FSBO listings
- Notification channels: email (phase 2), in-app feed
- Scraped portals: **4zida.rs, halooglasi.com, nadjidom.com, kupujemprodajem.com, nekretnine.rs**

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | .NET 10 Web API (ASP.NET Core) |
| Background Workers | .NET 10 Worker Service (separate project) |
| Frontend | Blazor WebAssembly (.NET 10) |
| UI Component Library | MudBlazor (dark/light theme support) |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core 9 |
| Auth | ASP.NET Identity + JWT Bearer tokens |
| AI Service | Google Gemini API (free tier in dev, paid in prod) |
| Email | MailKit (SMTP) |
| Scraping | HttpClient + HtmlAgilityPack |
| Orchestration / Automation | n8n (Docker) |
| Caching | In-memory (IMemoryCache) |
| Logging | Serilog → stdout + file sink |

---

## Solution Structure

```
SmartEstate/
├── CLAUDE.md                         ← this file (always read first)
├── .claude/
│   ├── sprint-plans.md               ← detailed sprint plans, issue trackers, API contracts
│   ├── lead-dev.md                   ← Lead Dev role instructions + retrospectives
│   ├── backend-dev.md                ← Backend Dev role instructions
│   └── frontend-dev.md               ← Frontend Dev role instructions
├── docker-compose.yml
├── SmartEstate.slnx
└── src/
    ├── SmartEstate.Domain/           ← entities, value objects, domain events
    ├── SmartEstate.Application/      ← CQRS (MediatR), interfaces, DTOs, business logic
    ├── SmartEstate.Infrastructure/   ← EF Core, repos, Gemini, email, scrapers
    ├── SmartEstate.API/              ← ASP.NET Core Web API, controllers, middleware, JWT
    ├── SmartEstate.Workers/          ← IHostedService workers (Lead-Gen scraping)
    └── SmartEstate.Web/              ← Blazor WASM client
```

**Project dependencies (Clean Architecture):**
```
Domain ← Application ← Infrastructure
                     ← API
                     ← Workers
Web → (calls API via HTTP)
```

---

## Multi-Tenancy Architecture

**Strategy:** Row-level tenancy — every tenant-scoped entity carries a `TenantId` (Guid).

- All tenant-scoped entities implement `ITenantEntity`
- EF Core **Global Query Filters** automatically scope all queries to the current tenant
- `ITenantContext` resolved in middleware from the JWT `tenant_id` claim
- `Tenant` entity itself is NOT scoped (it is the root)
- Tenant activation managed manually by Administrator — no self-serve billing

**Tenant lifecycle:** Administrator creates tenant → creates `AgencyManager` user → flips `IsActive`.

---

## Authentication & Authorization

**Mechanism:** ASP.NET Identity + JWT Bearer tokens (stateless)

| Role | Scope | Capabilities |
|---|---|---|
| `Administrator` | Global (cross-tenant) | Manage tenants, users, roles, system config |
| `AgencyManager` | Within own tenant | View agents' activity, reports, agency settings |
| `Agent` | Within own tenant | Manage buyers, properties, view matches/reports |

**JWT Claims:** `sub` (userId), `email`, `role`, `tenant_id`, `exp`  
**Important:** Administrator has no `tenant_id` claim. All other roles always have a valid `tenant_id`.

---

## Development Workflow (n8n Automation)

```
Owner plans sprint with Lead Dev
    ↓
Lead Dev creates GitHub Issues with labels: [backend] / [frontend]
    ↓
n8n detects new issue → triggers Backend Dev or Frontend Dev Claude session
    ↓
Developer implements → creates Pull Request
    ↓
n8n detects new PR → triggers Lead Dev session to review
    ↓
Lead Dev reviews, approves, merges to main
```

**GitHub Issue Labels:** `backend`, `frontend`, `bug`, `architecture`

---

## Coding Conventions

- **Architecture:** Clean Architecture strictly — no domain logic in controllers or infrastructure
- **CQRS:** MediatR for all commands/queries in Application layer
- **Validation:** FluentValidation in Application layer (pipeline behavior)
- **Error handling:** `ErrorOr` result pattern — no exceptions for business logic
- **API responses:** Consistent `ApiResponse<T>` wrapper — never use `Problem()`
- **No magic strings:** Use `const` or `nameof` everywhere
- **EF Core:** Use `IApplicationDbContext` directly in handlers — no additional repository abstraction
- **Comments:** Only when the WHY is non-obvious

---

## Environment Configuration

**Local Dev:**
- API: `https://localhost:7001` (Visual Studio launch profile)
- Web: `https://localhost:7002` (Visual Studio launch profile)
- Do NOT start with `dotnet run` unless explicitly asked — use Visual Studio
- PostgreSQL via Docker: `localhost:5432` (db: `smartestate`)
- pgAdmin: `http://localhost:5050` | n8n: `http://localhost:5678`

**Production:** Windows Server, PostgreSQL on separate machine, all secrets via environment variables.

---

## Architectural Decisions Log

| Date | Decision | Reason |
|---|---|---|
| 2026-04-22 | .NET 10 | Installed on dev machine; uses .slnx format |
| 2026-04-22 | Blazor WASM | API-first, mobile-friendly, decoupled deployment |
| 2026-04-22 | Row-level multi-tenancy | Simplest EF Core approach, sufficient for SaaS |
| 2026-04-22 | JWT (not sessions/cookies) | WASM client, stateless API |
| 2026-04-22 | Gemini AI | Free tier for development phase |
| 2026-04-22 | MudBlazor | Proven component library, dark/light mode built-in |
| 2026-04-22 | No SignalR initially | Reduce complexity; email + in-app polling sufficient for MVP |
| 2026-04-22 | Separate Workers project | Scrapers must run independently of API |
| 2026-04-23 | DataSeeder via `CreateAsyncScope()` | Runs once at startup — simpler than IHostedService for one-off seed |
| 2026-04-23 | EF Core query filters reference `tenantContext` field directly | Captured `Guid?` locals freeze at model-build time; field is re-evaluated per query |
| 2026-04-23 | Migrations output dir: `Persistence/Migrations` | Co-located with `AppDbContext` in Infrastructure |
| 2026-04-23 | `TenantContext` is a settable POCO | `TenantMiddleware` sets it explicitly — no `IHttpContextAccessor` dependency |
| 2026-04-23 | `IXxxService` pattern for Identity concerns | Keeps Application layer free of Identity types |
| 2026-04-23 | `GlobalExceptionHandler` in `API/Common/` | Catches `ValidationException` → 400, others → 500, always `ApiResponse` shape |
| 2026-04-23 | `ITenantCache` in Application, `TenantCache` in Infrastructure | Avoids leaking `IMemoryCache` into Application layer |
| 2026-04-23 | `Tenant.Plan` added via `AddTenantPlan` migration | Sprint 0 schema did not include Plan; added as nullable string |
| 2026-04-23 | JWT bearer as explicit default auth scheme | Prevents Identity cookie scheme from intercepting API bearer challenges |
| 2026-04-24 | `ISoftDeletable` interface in Domain | Enables generic soft-delete support; `Buyer` implements it; `Property` will too in Sprint 3 |
| 2026-04-24 | `Buyer` uses inline combined `HasQueryFilter` (tenant + soft-delete) | EF Core allows only one `HasQueryFilter` per entity — cannot chain; all conditions must be in one lambda |
| 2026-04-24 | `AssignedAgentId` set from JWT `sub` claim in controller | Never from client body — controller extracts via `User.FindFirstValue(ClaimTypes.NameIdentifier)` |
| 2026-04-24 | `PreferredLocations` migration requires `defaultValueSql: "ARRAY[]::text[]"` | PostgreSQL cannot add NOT NULL column to existing rows without a default |

---

## Sprint History

> For detailed issue trackers, API contracts, and implementation notes — see **`.claude/sprint-plans.md`**.

| Sprint | Status | Summary |
|---|---|---|
| Sprint 0 | ✅ Complete | Foundation — Docker, EF Core migration, Serilog, Blazor layout, CI pipeline |
| Sprint 1 | ✅ Complete | Auth (JWT login), multi-tenancy middleware, admin tenant/user management |
| Sprint 2 | 🔄 In Progress | Buyer CRUD — backend complete (#45–#48 ✅), frontend pending (#49–#51) |
| Sprint 3 | Planned | Property CRUD (backend + frontend) |
| Sprint 4 | Planned | Gemini AI tagging for buyers and properties |
| Sprint 5 | Planned | Matching engine + match reports |
| Sprint 6 | Planned | In-app notifications — Phase 1 complete |
| Sprint 7 | Planned | FSBO scrapers — all 5 portals |
| Sprint 8 | Planned | FSBO classification (Gemini) + lead notifications — Phase 2 complete |
| Sprint 9 | Planned | AgencyManager dashboard + agent management |
| Sprint 10 | Planned | Production readiness — IIS, secrets, security audit |

---

## Known Constraints & Important Notes

### Architecture
- `FsboLead` has **no `TenantId`** — global entity. `GET /fsbo-leads` per-tenant scoping must be resolved in Sprint 7 (create an `architecture` issue).
- EF Core Global Query Filters must reference the `ITenantContext` field directly — never capture `TenantId` as a local variable in `OnModelCreating`.
- `DataSeeder` is idempotent — runs on every startup, safe to re-run.

### Backend
- `SmartEstate.Domain` must not reference ASP.NET/Identity packages — current `Microsoft.AspNetCore.Identity 2.3.9` reference is unused and causes NU1903 warnings. Remove in a dedicated cleanup PR.
- `Serilog.Sinks.File` must be version `7.0.0` — lower versions cause NU1605 build error
- `Microsoft.EntityFrameworkCore.Design` must be in the **startup project** (API), not only Infrastructure
- Migration command: `dotnet ef migrations add <Name> --project src/SmartEstate.Infrastructure --startup-project src/SmartEstate.API`
- All migrations applied manually — no auto-migration on startup in production
- `Administrator` seeded from `ADMIN_EMAIL` / `ADMIN_PASSWORD` env vars; seeder skips with warning if not set
- CORS policy named `"BlazorWasm"` — targets `https://localhost:7002` locally; update `AllowedOrigins` for prod
- `System.IdentityModel.Tokens.Jwt 8.17.0` must be explicitly added to Infrastructure
- `UseSerilogRequestLogging()` must come **before** `UseExceptionHandler()` in `Program.cs`
- `AppClaims.TenantId` constant (`"tenant_id"`) in `Application/Common/Constants/AppClaims.cs` — use everywhere
- `TenantContext` double-registration: `AddScoped<TenantContext>()` + `AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>())`
- `TenantMiddleware` registered after `UseAuthorization()` in `Program.cs`
- Controller `Problem()` must NOT be used — always `StatusCode(500, ApiResponse.Fail(...))`
- API request body records in `API/Models/Requests/` — one file per record
- API skips `UseHttpsRedirection()` in Development only

### Frontend
- `wwwroot/appsettings.Development.json` is gitignored — configure locally after clone
- MudBlazor 9.x: `GetSystemPreference()` removed — default to `false` (light mode)
- MudBlazor 9.x: required providers: `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider`, `MudSnackbarProvider`
- Theme key in localStorage: `smartestate_dark_mode` — read in `OnAfterRenderAsync`, not `OnInitializedAsync`
- Login redirects are role-aware: `Administrator` → `/admin/tenants`, tenant users → `/dashboard`
- Feature services (e.g. `TenantAdminService`, `BuyerService`) registered in `Program.cs`
- Admin nav links inside `<AuthorizeView Roles="Administrator">` only

### CI / GitHub
- GitHub Actions: `.github/workflows/ci.yml` — triggers on PR to `main`, job: `Build & Test`
- Branch protection requires `Build & Test` to pass before merge
- Lead Dev uses `gh pr merge --admin` when CI hasn't run yet on arch PRs

### Local Test Accounts
- `admin@smartestate.local` — global `Administrator`
- `manager.active@smartestate.local` — active tenant `Demo Realty Active`
- `manager.inactive@smartestate.local` — inactive tenant (API returns 403 on tenant-scoped calls)
- Do not document passwords in tracked files
