# SmartEstate — Shared Project Knowledge Base

> This file is the **single source of truth** for all AI sessions working on this project.
> Every session (Lead Dev, Backend Dev, Frontend Dev) **must read this file in full before starting any work**.
> Every session **must update this file** when making architectural decisions, discovering important constraints, or completing significant milestones.

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
- Agents enter buyer profiles with free-text lifestyle preferences (not just square meters — lifestyle, habits, priorities)
- AI (Gemini) analyzes buyer descriptions and assigns semantic tags + structured profile
- Agents enter property listings with descriptions and images
- AI analyzes properties and assigns tags
- System matches buyers to properties based on tag/profile similarity
- Agents have a matching report page where they log buyer reactions
- AI continuously refines matching quality based on feedback history

### Phase 2 — Lead-Gen (FSBO Detection)
- Background workers continuously scrape real estate portals for new listings
- AI classifies listings as agency-posted vs. private-seller (FSBO)
- When FSBO is detected, active/subscribed agencies are immediately notified
- Notification channels: email (phase 2 launch), in-app feed
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
| Caching | In-memory (IMemoryCache) for now |
| Logging | Serilog → stdout + file sink |

---

## Solution Structure

```
SmartEstate/
├── CLAUDE.md                         ← this file
├── .claude/
│   ├── lead-dev.md
│   ├── backend-dev.md
│   └── frontend-dev.md
├── docker-compose.yml                ← local dev: PostgreSQL, pgAdmin, n8n
├── .gitignore
├── SmartEstate.slnx          ← .NET 10 solution format
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

**Key rules:**
- All tenant-scoped entities implement `ITenantEntity` (exposes `TenantId`)
- EF Core **Global Query Filters** automatically scope all queries to the current tenant
- `ITenantContext` service is resolved in middleware from the JWT `tenant_id` claim
- Tenant activation is managed manually by Administrator (no self-serve billing)
- `Tenant` entity itself is NOT scoped (it is the root)

**Tenant lifecycle:**
1. Administrator creates tenant record (agency name, contact, plan)
2. Administrator creates initial `AgencyManager` user for that tenant
3. Tenant `IsActive` flag controls access — Administrator flips it manually

---

## Authentication & Authorization

**Mechanism:** ASP.NET Identity + JWT Bearer tokens (stateless)

**Roles:**
| Role | Scope | Capabilities |
|---|---|---|
| `Administrator` | Global (cross-tenant) | Manage tenants, users, roles, system config |
| `AgencyManager` | Within own tenant | View agents' activity, reports, agency settings |
| `Agent` | Within own tenant | Manage buyers, properties, view matches/reports |

**JWT Claims:** `sub` (userId), `email`, `role`, `tenant_id`, `exp`

**Important:** Administrator has no `tenant_id` claim (or a null/special value). All other roles always have a valid `tenant_id`.

---

## Notifications

- **Phase 1:** In-app notification feed only (stored in DB, polled/loaded on page visit — no real-time push)
- **Phase 2 / Lead-Gen launch:** Email via MailKit + in-app feed
- No SignalR / WebSockets in initial phases

---

## Development Workflow (n8n Automation)

```
Owner plans sprint with Lead Dev
    ↓
Lead Dev creates GitHub Issues with labels: [backend] / [frontend]
    ↓
n8n detects new issue with [backend] label → triggers Backend Dev Claude session
n8n detects new issue with [frontend] label → triggers Frontend Dev Claude session
    ↓
Developer implements → creates Pull Request
    ↓
n8n detects new PR → triggers Lead Dev session to review
    ↓
Lead Dev reviews, approves, merges to main
    ↓
Developer checks: if no more open issues with their label → stops working
    ↓
Owner reviews milestone → plans next sprint with Lead Dev
```

**GitHub Issue Labels:**
- `backend` — Backend Dev picks up
- `frontend` — Frontend Dev picks up
- `bug` — can be combined with above
- `architecture` — Lead Dev handles directly

---

## Coding Conventions

- **Language:** C# (.NET 10), Razor (Blazor)
- **Naming:** PascalCase for classes/methods, camelCase for private fields, `_prefix` for private fields
- **Architecture:** Clean Architecture strictly — no domain logic in controllers or infrastructure
- **CQRS:** MediatR for all commands/queries in Application layer
- **Validation:** FluentValidation in Application layer (pipeline behavior)
- **Error handling:** Result pattern (no exceptions for business logic) — use `Result<T>` or `OneOf`
- **EF Core:** Repository pattern via `IRepository<T>` in Application, implemented in Infrastructure
- **API responses:** Consistent `ApiResponse<T>` wrapper
- **No magic strings:** Use `const` or `nameof` everywhere
- **Comments:** Only when the WHY is non-obvious

---

## Environment Configuration

**Local Dev:**
- API runs on `https://localhost:7001`
- Web (Blazor WASM) runs on `https://localhost:7002`  
- Workers run independently
- PostgreSQL via Docker: `localhost:5432` (db: `smartestate`, user: `smartestate`, pass: see docker-compose)
- pgAdmin via Docker: `http://localhost:5050`
- n8n via Docker: `http://localhost:5678`

**Production:**
- Windows Server (existing hosting)
- PostgreSQL on separate machine (not Docker)
- Connection strings via environment variables / secrets

---

## Architectural Decisions Log

| Date | Decision | Reason |
|---|---|---|
| 2026-04-22 | .NET 10 (not .NET 9) | .NET 10 is installed on dev machine; uses .slnx format |
| 2026-04-22 | Blazor WASM (not Server) | API-first, mobile-friendly, decoupled deployment |
| 2026-04-22 | Row-level multi-tenancy | Simplest EF Core approach, sufficient isolation for SaaS |
| 2026-04-22 | JWT (not sessions/cookies) | WASM client, stateless API |
| 2026-04-22 | Gemini AI | Free tier for development phase |
| 2026-04-22 | MudBlazor | Proven component library, dark/light mode built-in |
| 2026-04-22 | No SignalR initially | Reduce complexity; email + in-app polling sufficient for MVP |
| 2026-04-22 | Separate Workers project | Scrapers must run independently of API, different scaling needs |
| 2026-04-23 | DataSeeder via `CreateAsyncScope()` (not IHostedService) | Runs once at startup, simpler than a full IHostedService for a one-off seed operation |
| 2026-04-23 | EF Core query filters reference `tenantContext` field directly | Capturing `TenantId` as a local `Guid?` in `OnModelCreating` freezes the value at model-build time; the field reference is re-evaluated per query |
| 2026-04-23 | Migrations output dir: `Persistence/Migrations` | Keeps migrations co-located with `AppDbContext` in the Infrastructure persistence folder |

---

## Sprint History

*(Updated by Lead Dev after each sprint)*

| Sprint | Status | Summary |
|---|---|---|
| Sprint 0 | ✅ Complete | Foundation — Docker, EF Core migration, Serilog, Blazor layout, CI pipeline |
| Sprint 1 | 🔄 In Progress | Auth (JWT login), multi-tenancy middleware, admin tenant/user management |
| Sprint 2 | Planned | Buyer CRUD (backend + frontend) |
| Sprint 3 | Planned | Property CRUD (backend + frontend) |
| Sprint 4 | Planned | Gemini AI tagging for buyers and properties |
| Sprint 5 | Planned | Matching engine + match reports (backend + frontend) |
| Sprint 6 | Planned | In-app notifications — Phase 1 complete |
| Sprint 7 | Planned | FSBO scrapers — all 5 portals, Workers project |
| Sprint 8 | Planned | FSBO classification (Gemini) + lead notifications (in-app + email) — Phase 2 complete |
| Sprint 9 | Planned | AgencyManager dashboard + agent management |
| Sprint 10 | Planned | Production readiness — IIS, secrets, migrations, security audit |

---

## Full Sprint Plan

> This is the canonical sprint plan agreed with the Product Owner on 2026-04-23.
> Lead Dev maintains this. Update issue numbers as they are created. Mark sprints as In Progress / Complete as work progresses.
> **Critical path:** S0 → S1 → S2+S3 (parallel) → S4 → S5 → S6 → S7 → S8 → S9 → S10

---

### Sprint 0 — Foundation `[✅ Complete]`

**Goal:** Everything that must be in place before any feature work can begin.

| # | GitHub Issue | Task | Label | Status |
|---|---|---|---|---|
| 0.1 | [#13](https://github.com/LazarVujosevic/SmartEstate/issues/13) | Docker Compose — PostgreSQL 16, pgAdmin, n8n | `backend` | ✅ Done |
| 0.2 | [#14](https://github.com/LazarVujosevic/SmartEstate/issues/14) | EF Core initial migration + Administrator seed (from env vars) | `backend` | ✅ Done |
| 0.3 | [#15](https://github.com/LazarVujosevic/SmartEstate/issues/15) | Serilog configuration — stdout + rolling file sink, both API and Workers | `backend` | ✅ Done |
| 0.4 | [#16](https://github.com/LazarVujosevic/SmartEstate/issues/16) | Blazor WASM starter layout — MudBlazor theme, nav drawer, dark/light toggle | `frontend` | ✅ Done |
| 0.5 | [#17](https://github.com/LazarVujosevic/SmartEstate/issues/17) | GitHub Actions CI — build + test on every PR, branch protection on main | `architecture` | ✅ Done |

**Already completed before Sprint 0 issues were created:**
- Solution structure (all 6 projects scaffolded)
- Domain entities: `Tenant`, `Buyer`, `Property`, `FsboLead`, `InAppNotification`, `MatchReport`
- Domain enums: `BuyerReaction`, `FsboLeadStatus`, `PortalSource`, `PropertyStatus`, `PropertyType`
- `BaseEntity`, `ITenantEntity`, `ValidationBehavior`, `ApiResponse<T>`
- Application interfaces: `IApplicationDbContext`, `ITenantContext`, `IAITaggingService`, `IEmailService`
- `AppDbContext` skeleton, `ApplicationUser`, `ApplicationRole`
- `/health` and `/ping` endpoints (issues #3, #6)

---

### Sprint 1 — Auth & Multi-Tenancy `[🔄 In Progress]`

**Goal:** No one can do anything without auth. This sprint is a hard blocker for all feature sprints.
**Depends on:** Sprint 0 complete ✅.

> **Note for Lead Dev:** When creating Sprint 1 GitHub Issues, skip "Route guard — AuthorizeRouteView + RedirectToLogin" — already implemented in PR #22 (Sprint 0 Frontend). The Blazor WASM routing infrastructure is done; Sprint 1 Frontend only needs Login page + JwtAuthStateProvider.

**Backend tasks:**
| Task | Label |
|---|---|
| POST `/auth/login` — ASP.NET Identity + JWT generation (claims: sub, email, role, tenant_id, exp) | `backend` |
| `TenantMiddleware` — resolves `ITenantContext` from JWT `tenant_id` claim on every request | `backend` |
| EF Core Global Query Filters for all `ITenantEntity` entities in `AppDbContext` | `backend` |
| Middleware: block requests from inactive tenants (return 403), except Administrator | `backend` |
| Admin API: POST `/admin/tenants` — create a new tenant (name, contact, plan) | `backend` |
| Admin API: POST `/admin/tenants/{id}/users` — create initial AgencyManager user for tenant | `backend` |
| Admin API: PATCH `/admin/tenants/{id}/activate` — toggle `IsActive` flag | `backend` |

**Frontend tasks:**
| Task | Label |
|---|---|
| Login page — MudBlazor form, JWT stored in localStorage, redirect after login | `frontend` |
| `JwtAuthStateProvider` — custom `AuthenticationStateProvider`, parses JWT claims from stored JWT | `frontend` |
| Fix `_isDarkMode` field initializer in `MainLayout.razor` — change default from `true` to `false` to avoid dark→light flash on first load | `frontend` |
| Admin panel — tenant list (MudDataGrid), create tenant form, toggle activation | `frontend` |

---

### Sprint 2 — Buyer Management `[Planned]`

**Goal:** Agents can create and manage buyer profiles with lifestyle descriptions.
**Depends on:** Sprint 1 complete (auth + tenant middleware required).
**Can run in parallel with:** Sprint 3.

**Backend tasks:**
| Task | Label |
|---|---|
| POST `/buyers` — create buyer (name, lifestyle description, budget range, preferred locations) | `backend` |
| GET `/buyers` — paginated list for current tenant | `backend` |
| GET `/buyers/{id}` — buyer details | `backend` |
| PUT `/buyers/{id}` — update buyer | `backend` |
| DELETE `/buyers/{id}` — soft delete (`IsDeleted` flag, filtered by default) | `backend` |

**Frontend tasks:**
| Task | Label |
|---|---|
| Buyer list page — MudDataGrid with pagination, search by name | `frontend` |
| Buyer create/edit form — modal or dedicated page | `frontend` |
| Buyer detail page — full profile view | `frontend` |

---

### Sprint 3 — Property Management `[Planned]`

**Goal:** Agents can create and manage property listings with descriptions.
**Depends on:** Sprint 1 complete.
**Can run in parallel with:** Sprint 2.

**Backend tasks:**
| Task | Label |
|---|---|
| POST `/properties` — create property (address, type, area, price, description) | `backend` |
| GET `/properties` — paginated list for current tenant, filterable by `PropertyStatus` | `backend` |
| GET `/properties/{id}` — property details | `backend` |
| PUT `/properties/{id}` — update property | `backend` |
| PATCH `/properties/{id}/status` — change status (Available → UnderContract → Sold) | `backend` |
| DELETE `/properties/{id}` — soft delete | `backend` |

**Frontend tasks:**
| Task | Label |
|---|---|
| Property list page — MudDataGrid, status filter chips | `frontend` |
| Property create/edit form | `frontend` |
| Property detail page | `frontend` |

---

### Sprint 4 — AI Tagging via Gemini `[Planned]`

**Goal:** Gemini analyzes buyer descriptions and property descriptions, assigns semantic tags. Core of Phase 1.
**Depends on:** Sprint 2 and Sprint 3 complete.

**Backend tasks:**
| Task | Label |
|---|---|
| `GeminiTaggingService` — implements `IAITaggingService` using Google Generative AI SDK | `backend` |
| MediatR Command: `TagBuyerCommand` — sends buyer description to Gemini, persists returned tags on `Buyer` | `backend` |
| MediatR Command: `TagPropertyCommand` — sends property description to Gemini, persists tags on `Property` | `backend` |
| Auto-trigger tagging on Buyer create/update and Property create/update (MediatR pipeline or domain event) | `backend` |
| Exponential backoff retry for Gemini 429 rate-limit errors | `backend` |
| Debug endpoints: GET `/buyers/{id}/tags`, GET `/properties/{id}/tags` | `backend` |

> **Architecture note:** Tags stored as JSON column on the entity for MVP simplicity. Each tag has a `name` and `weight` (float). Gemini prompt must return structured JSON.

---

### Sprint 5 — Matching Engine `[Planned]`

**Goal:** System proposes which buyers match which properties based on AI tags.
**Depends on:** Sprint 4 complete.

**Backend tasks:**
| Task | Label |
|---|---|
| `MatchingService` — computes similarity score between buyer tags and property tags (weighted Jaccard) | `backend` |
| GET `/properties/{id}/matches` — ranked list of buyers matching that property (with score) | `backend` |
| GET `/buyers/{id}/matches` — ranked list of properties matching that buyer (with score) | `backend` |
| POST `/match-reports` — agent logs buyer reaction (`BuyerReaction` enum: Interested, NotInterested, Visited, Offered) | `backend` |
| GET `/match-reports` — list of all match reports for tenant | `backend` |
| Feedback loop: periodically batch-send reaction history to Gemini to refine buyer/property tags | `backend` |

**Frontend tasks:**
| Task | Label |
|---|---|
| Match page for property — ranked buyer list with score and reaction action | `frontend` |
| Match page for buyer — ranked property list | `frontend` |
| Log reaction form — agent selects reaction after showing property to buyer | `frontend` |
| Match reports list — AgencyManager view of all reactions | `frontend` |

---

### Sprint 6 — In-App Notifications `[Planned]`

**Goal:** Users receive in-app notifications for relevant events. Completes Phase 1.
**Depends on:** Sprint 5 complete.

**Backend tasks:**
| Task | Label |
|---|---|
| `NotificationService` — creates `InAppNotification` records (type, message, targetUserId, isRead, createdAt) | `backend` |
| GET `/notifications` — list of unread notifications for currently authenticated user | `backend` |
| PATCH `/notifications/{id}/read` — mark notification as read | `backend` |
| Auto-create notification when a match score exceeds configured threshold | `backend` |

**Frontend tasks:**
| Task | Label |
|---|---|
| Notification bell icon in app bar with unread badge count | `frontend` |
| Notification dropdown panel — list with mark-as-read action | `frontend` |
| Polling mechanism — GET `/notifications` every 60 seconds | `frontend` |

> **Phase 1 complete after this sprint.** AgencyManager sees everything, Agents manage buyers/properties, AI tags and matches, notifications fire.

---

### Sprint 7 — FSBO Scrapers `[Planned]`

**Goal:** Workers project continuously scrapes 5 real estate portals and stores raw listings.
**Depends on:** Sprint 0 complete (Workers infrastructure). Can be developed in parallel with S5/S6.

**Backend tasks:**
| Task | Label |
|---|---|
| `ScraperWorkerBase` — abstract base with `PeriodicTimer` loop, error handling, Serilog logging, `CancellationToken` | `backend` |
| `FsboLeadRepository` — persist scraped listings, duplicate detection by external URL | `backend` |
| Scraper: **4zida.rs** — HttpClient + HtmlAgilityPack, extracts title, price, location, URL, description | `backend` |
| Scraper: **halooglasi.com** | `backend` |
| Scraper: **nadjidom.com** | `backend` |
| Scraper: **kupujemprodajem.com** | `backend` |
| Scraper: **nekretnine.rs** | `backend` |
| Graceful degradation — if scraper throws (HTML changed), log full context + continue, never crash worker | `backend` |

> **Architecture note:** Each portal = separate `IHostedService`. Scraping interval configured in `appsettings.json` per scraper. Raw listing data is stored before AI classification to allow replay if classification fails.

---

### Sprint 8 — FSBO Classification & Lead Notifications `[Planned]`

**Goal:** Gemini classifies scraped listings as FSBO or agency-posted. Agencies are notified of FSBOs. Completes Phase 2.
**Depends on:** Sprint 7 complete, Sprint 6 complete (notification infrastructure).

**Backend tasks:**
| Task | Label |
|---|---|
| `FsboClassificationService` — batch sends scraped listings to Gemini, classifies agency vs FSBO | `backend` |
| Batch classification logic — collect N unclassified listings, send in one Gemini call (avoid rate limits) | `backend` |
| On FSBO detection: create `InAppNotification` for all active AgencyManagers across all tenants | `backend` |
| Email notification via MailKit — send FSBO alert email to AgencyManager email address | `backend` |
| GET `/fsbo-leads` — paginated list of FSBO leads for tenant, filterable by `FsboLeadStatus` | `backend` |
| PATCH `/fsbo-leads/{id}/status` — agent updates status (New → Contacted → Converted / Dismissed) | `backend` |

**Frontend tasks:**
| Task | Label |
|---|---|
| FSBO Leads list page — MudDataGrid with status filter, urgency sorting by detected date | `frontend` |
| FSBO Lead detail page — listing info, portal link, status change action | `frontend` |
| Email notification settings form for AgencyManager (SMTP config per tenant) | `frontend` |

> **Phase 2 complete after this sprint.**

---

### Sprint 9 — AgencyManager Dashboard `[Planned]`

**Goal:** AgencyManager has a unified overview of agency activity and KPIs.
**Depends on:** Sprint 8 complete.

**Backend tasks:**
| Task | Label |
|---|---|
| GET `/dashboard/summary` — KPIs: active buyers, active properties, matches this week, new FSBO leads | `backend` |
| GET `/dashboard/agent-activity` — per-agent breakdown: entries created, reactions logged | `backend` |

**Frontend tasks:**
| Task | Label |
|---|---|
| AgencyManager dashboard page — MudCard KPI tiles, recent activity feed | `frontend` |
| Agent management page — list agents, invite by email, deactivate | `frontend` |
| Agency settings page — name, contact info, notification preferences | `frontend` |

---

### Sprint 10 — Production Readiness `[Planned]`

**Goal:** Safe deployment to Windows Server. Nothing breaks in production.
**Depends on:** All previous sprints complete.

| Task | Label |
|---|---|
| Environment variable configuration for all secrets (connection string, JWT key, Gemini API key, SMTP) | `backend` |
| EF Core migration apply script + runbook documentation for production deploys | `architecture` |
| IIS `web.config` for API (reverse proxy, HTTPS redirect) | `backend` |
| Blazor WASM publish as static files under IIS | `backend` |
| Serilog file sink configuration for production (log rotation, 30-day retention) | `backend` |
| Database index audit — review all queries for N+1 issues, add missing indexes | `backend` |
| Health check endpoint upgrade — verify DB connection, not just ping | `backend` |
| Security audit — CORS policy, HTTPS enforce, security response headers, no secrets in code | `backend` |

---

## Known Constraints & Important Notes

### Architecture
- `FsboLead` entity has **no `TenantId`** — it is a global entity (scraped once, shared across all agencies). Sprint 7/8 must resolve how `GET /fsbo-leads` is scoped per tenant: either via an `InAppNotification` reference, or a `TenantFsboLead` junction table. Create an `architecture` issue when starting Sprint 7.
- EF Core Global Query Filters must reference the `ITenantContext` service field directly, not a captured `Guid?` local variable — captured locals are frozen at model-build time; field references are re-evaluated per query.
- `DataSeeder` is idempotent and runs on every API startup — it checks existence before creating. Safe to re-run.

### Backend
- `Serilog.Sinks.File` must be version `7.0.0` — `Serilog.AspNetCore 10.0.0` has a transitive dependency on `>= 7.0.0`; using 6.0.0 causes NU1605 build error
- `Microsoft.EntityFrameworkCore.Design` must be in the **startup project** (API), not only in Infrastructure — EF Core CLI tools require it on the startup project to generate migrations
- `dotnet-ef` global tool must be installed before running any migration commands: `dotnet tool install --global dotnet-ef`
- Migration command: `dotnet ef migrations add <Name> --project src/SmartEstate.Infrastructure --startup-project src/SmartEstate.API`
- All database migrations must be applied manually (no auto-migration on startup in production)
- `Administrator` role is seeded on first run — credentials read from `ADMIN_EMAIL` / `ADMIN_PASSWORD` env vars (or `appsettings.Development.json`); if not set, seeder skips with a warning log
- CORS policy in API is named `"BlazorWasm"` — configured for `https://localhost:7002` in dev; update `AllowedOrigins` in `appsettings.json` for prod
- Scraping portals (4zida, halooglasi, etc.) may change their HTML structure — scrapers must fail gracefully, log full context, and continue; never crash the worker
- `System.IdentityModel.Tokens.Jwt 8.17.0` must be explicitly added to Infrastructure — it is NOT transitively available via `FrameworkReference Include="Microsoft.AspNetCore.App"` even though JwtBearer is in the framework
- `UseSerilogRequestLogging()` must come **before** `UseExceptionHandler()` in `Program.cs` — otherwise Serilog does not capture the rewritten status code from exception handling
- `GlobalExceptionHandler` (implements `IExceptionHandler`) is in `API/Common/` — catches `FluentValidation.ValidationException` from the MediatR pipeline and returns `400 Bad Request` with `ApiResponse` shape; catches all other exceptions and returns `500`
- `IAuthService` pattern: when Application handlers need Identity concerns (`UserManager` etc.), define `IXxxService` in Application and implement in Infrastructure — keeps Application layer free of Identity types
- `JwtSettings` options class at `Infrastructure/Identity/JwtSettings.cs`, bound via `services.Configure<JwtSettings>(configuration.GetSection("Jwt"))` — properties: `Secret`, `Issuer`, `Audience`, `ExpiryMinutes` (default 60)
- JWT `Secret` is validated at startup (≥ 32 chars) — app throws `InvalidOperationException` if missing or too short; placeholder in `appsettings.json` is long enough for dev

### Frontend
- Docker Compose credentials are in `.env` (gitignored) — copy `.env.example` to `.env` before first `docker compose up -d`
- `wwwroot/appsettings.Development.json` is gitignored — only `wwwroot/appsettings.json` is tracked; configure local overrides manually after clone
- MudBlazor 9.x: `MudThemeProvider.GetSystemPreference()` no longer exists — do not use it; default to `false` (light mode) if no localStorage value
- MudBlazor 9.x: required providers in layout are `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider`, `MudSnackbarProvider` — all four must be present or dropdowns/dialogs/snackbars will silently fail
- Theme preference is stored in `localStorage` under key `smartestate_dark_mode` (bool) — read in `OnAfterRenderAsync`, not `OnInitializedAsync` (localStorage is unavailable during prerender)
- `AuthorizeRouteView` + `RedirectToLogin` are already wired in `App.razor` — Sprint 1 Frontend only needs to register `JwtAuthStateProvider` and build the Login page
- Gemini API has rate limits on free tier — batch AI requests where possible, avoid per-request calls in loops
- Windows Server production deployment: use IIS reverse proxy for API + publish Blazor WASM as static files

### CI / GitHub
- GitHub Actions workflow: `.github/workflows/ci.yml` — triggers on PR to `main`, job name is `Build & Test`
- Branch protection on `main` requires `Build & Test` status check to pass before merge
- GitHub does not allow self-approve on PRs — Lead Dev uses `gh pr merge --admin` when CI hasn't run yet on infrastructure/arch PRs
