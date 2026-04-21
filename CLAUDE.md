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

---

## Sprint History

*(Updated by Lead Dev after each sprint)*

| Sprint | Status | Summary |
|---|---|---|
| Sprint 0 | Planning | Initial setup — solution structure, auth scaffold, docker, CI |

---

## Known Constraints & Important Notes

- Scraping portals (4zida, halooglasi, etc.) may change their HTML structure — scrapers must be designed to fail gracefully and alert via logs, not crash the worker
- Gemini API has rate limits on free tier — batch AI requests where possible, avoid per-request calls in loops
- `Administrator` role is seeded on first run — credentials must be set via environment variable, not hardcoded
- All database migrations must be applied manually (no auto-migration on startup in production)
- Windows Server production deployment: use IIS reverse proxy for API + publish Blazor WASM as static files
