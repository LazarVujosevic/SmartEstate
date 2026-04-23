# SmartEstate — Full Sprint Plan

> **Every session (Backend Dev, Frontend Dev, Lead Dev) must read this file when starting work on any sprint.**
> Lead Dev maintains this file. Update issue numbers, statuses, and notes as work progresses.
> **Critical path:** S0 → S1 → S2 → S3 → S4 → S5 → S6 → S7 → S8 → S9 → S10

---

## Sprint 0 — Foundation `[✅ Complete]`

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

---

## Sprint 1 — Auth & Multi-Tenancy `[✅ Complete]`

**Goal:** No one can do anything without auth. Hard blocker for all feature sprints.

| # | GitHub Issue | Task | Label | Status |
|---|---|---|---|---|
| 1.1 | [#23](https://github.com/LazarVujosevic/SmartEstate/issues/23) | POST `/auth/login` — ASP.NET Identity + JWT generation | `backend` | ✅ Done |
| 1.2 | [#24](https://github.com/LazarVujosevic/SmartEstate/issues/24) | `TenantMiddleware` — resolves `ITenantContext` from JWT `tenant_id` claim | `backend` | ✅ Done |
| 1.3 | [#25](https://github.com/LazarVujosevic/SmartEstate/issues/25) | EF Core Global Query Filters for all `ITenantEntity` entities | `backend` | ✅ Done |
| 1.4 | [#26](https://github.com/LazarVujosevic/SmartEstate/issues/26) | Middleware: block requests from inactive tenants (return 403) | `backend` | ✅ Done |
| 1.5 | [#27](https://github.com/LazarVujosevic/SmartEstate/issues/27) | Admin API: POST `/admin/tenants` — create a new tenant | `backend` | ✅ Done |
| 1.6 | [#28](https://github.com/LazarVujosevic/SmartEstate/issues/28) | Admin API: POST `/admin/tenants/{id}/users` — create AgencyManager | `backend` | ✅ Done |
| 1.7 | [#29](https://github.com/LazarVujosevic/SmartEstate/issues/29) | Admin API: PATCH `/admin/tenants/{id}/activate` — toggle `IsActive` | `backend` | ✅ Done |
| 1.8 | [#30](https://github.com/LazarVujosevic/SmartEstate/issues/30) | `JwtAuthStateProvider` — custom `AuthenticationStateProvider` | `frontend` | ✅ Done |
| 1.9 | [#31](https://github.com/LazarVujosevic/SmartEstate/issues/31) | Login page — MudBlazor form, JWT stored in localStorage | `frontend` | ✅ Done |
| 1.10 | [#32](https://github.com/LazarVujosevic/SmartEstate/issues/32) | Fix `_isDarkMode` field initializer in `MainLayout.razor` | `frontend` | ✅ Done |
| 1.11 | [#33](https://github.com/LazarVujosevic/SmartEstate/issues/33) | Admin panel — tenant list, create form, activation toggle | `frontend` | ✅ Done |

---

## Sprint 2 — Buyer Management `[🔄 In Progress]`

**Goal:** Agents can create and manage buyer profiles with lifestyle descriptions.
**Depends on:** Sprint 1 complete ✅

### Issue tracker

| # | GitHub Issue | Task | Label | Status |
|---|---|---|---|---|
| 2.1 | [#45](https://github.com/LazarVujosevic/SmartEstate/issues/45) | Buyer schema + API contract foundation | `backend` | ✅ Done (PR #52) |
| 2.2 | [#46](https://github.com/LazarVujosevic/SmartEstate/issues/46) | POST `/buyers` + GET `/buyers` paginated list | `backend` | ✅ Done (PR #53) |
| 2.3 | [#47](https://github.com/LazarVujosevic/SmartEstate/issues/47) | GET `/buyers/{id}` + PUT `/buyers/{id}` | `backend` | ✅ Done (PR #54) |
| 2.4 | [#48](https://github.com/LazarVujosevic/SmartEstate/issues/48) | DELETE `/buyers/{id}` soft delete | `backend` | ✅ Done (PR #55) |
| 2.5 | [#49](https://github.com/LazarVujosevic/SmartEstate/issues/49) | Buyer models/service + navigation entry | `frontend` | 🔄 PR #56 (awaiting review) |
| 2.6 | [#50](https://github.com/LazarVujosevic/SmartEstate/issues/50) | Buyer list page with MudDataGrid, pagination, search | `frontend` | 🔄 PR #57 (awaiting review) |
| 2.7 | [#51](https://github.com/LazarVujosevic/SmartEstate/issues/51) | Buyer create/edit/detail UI | `frontend` | 🔄 PR #58 (awaiting review) |

### Dependency order
- Backend: #45 first → then #46, #47, #48 in parallel
- Frontend: #49 first → then #50 → then #51

### API contract

| Method | Route | Roles | Purpose |
|---|---|---|---|
| POST | `/api/buyers` | `Agent`, `AgencyManager` | Create buyer |
| GET | `/api/buyers` | `Agent`, `AgencyManager` | Paginated buyer list |
| GET | `/api/buyers/{id}` | `Agent`, `AgencyManager` | Buyer details |
| PUT | `/api/buyers/{id}` | `Agent`, `AgencyManager` | Update buyer |
| DELETE | `/api/buyers/{id}` | `Agent`, `AgencyManager` | Soft delete buyer |

### Backend implementation notes

**#45 — Buyer schema + API contract foundation**
- Add to `Buyer` entity: `BudgetMinEur` (decimal?), `BudgetMaxEur` (decimal?), `PreferredLocations` (List<string>), `IsDeleted` (bool, default false)
- EF Core migration — existing rows remain valid (nullable budget, empty list, IsDeleted = false)
- Global query filter on `Buyer` must exclude `IsDeleted == true` while tenant filter still applies
- Add `BuyerDto`, `BuyerListItemDto` in `Application/Features/Buyers/DTOs/`
- Add `CreateBuyerCommand`, `UpdateBuyerCommand` in `Application/Features/Buyers/Commands/`
- Add `PagedResult<T>` in `Application/Common/Models/` if not present
- `AssignedAgentId` comes from JWT `sub` claim — never from client input
- Add DB index on `(TenantId, IsDeleted, FullName)` in the migration
- AI fields (`AiTags`, `AiProfile`, `LastAiProcessedAt`) — do NOT touch, reserved for Sprint 4

**#46 — POST + GET /buyers**
- `POST /api/buyers`: required fields `fullName`, `lifestyleDescription`; optional `email`, `phone`, `budgetMinEur`, `budgetMaxEur`, `preferredLocations`
- FluentValidation: fullName max 200, lifestyleDescription max 4000, email format if present, phone max 50, budgets non-negative, min <= max
- `GET /api/buyers?pageNumber=1&pageSize=20&search=...`: search matches fullName, email, phone, preferredLocations
- Response: `ApiResponse<PagedResult<BuyerListItemDto>>`

**#47 — GET + PUT /buyers/{id}**
- GET returns 404 if not found, wrong tenant, or soft-deleted
- PUT: editable fields only — `fullName`, `email`, `phone`, `lifestyleDescription`, `budgetMinEur`, `budgetMaxEur`, `preferredLocations`; sets `UpdatedAt`
- PUT must NOT allow changing `TenantId`, `AssignedAgentId`, AI fields, `IsDeleted`
- Response: `ApiResponse<BuyerDto>`

**#48 — DELETE /buyers/{id}**
- Sets `IsDeleted = true`, `UpdatedAt = DateTime.UtcNow` — no physical delete
- Returns `Ok(ApiResponse<object>.Ok("Buyer deleted"))` — no 204
- Returns 404 if already deleted (excluded by filter)
- `MatchReport` records are NOT deleted — Sprint 5 depends on them

**General backend constraints:**
- Use `IApplicationDbContext` directly — no new repository abstraction
- Tenant isolation via global query filter — never use `.IgnoreQueryFilters()`
- No `Problem()` — always `ApiResponse<T>`
- Use `AppClaims` constants — no string literals for claim names or role names
- Controller in `API/Controllers/BuyersController.cs`
- **AppDbContext query filter pattern:** EF Core allows only one `HasQueryFilter` per entity. `Buyer` uses an inline combined filter (tenant + soft-delete). When Sprint 3 adds `ISoftDeletable` to `Property`, replace `ApplyTenantFilter<Property>` with the same inline pattern.

### Frontend implementation notes

**#49 — Models/service + nav**
- DTOs in `Web/Models/Buyers/`: `BuyerDto`, `BuyerListItemDto`, `CreateBuyerRequest`, `UpdateBuyerRequest`
- `PagedResult<T>` in `Web/Models/Common/` if not present
- `BuyerService` in `Web/Services/` — wraps all 5 buyer endpoints via `ApiClient`
- Register `BuyerService` in `Program.cs`
- Buyers nav in `NavMenu.razor` inside `<AuthorizeView Roles="Agent,AgencyManager">` — not visible to `Administrator`

**#50 — Buyer list page**
- Route: `/buyers`, `[Authorize(Roles = "Agent,AgencyManager")]`
- `MudDataGrid` with server-side pagination
- Columns: Full Name, Email, Phone, Budget Range, Preferred Locations, Created Date
- Debounced search input, loading/empty/error states
- Row actions: View, Edit, Delete (with confirmation dialog)
- After delete: snackbar + list refresh

**#51 — Buyer create/edit/detail UI**
- Detail page: `/buyers/{id:guid}` — read-only layout of all `BuyerDto` fields
- Create/edit: MudDialog or dedicated page — match existing patterns
- Fields: Full Name (required), Email, Phone, Lifestyle Description (required, multiline), Budget Min/Max, Preferred Locations (chips or comma-separated)
- Client-side validation: required fields, email format, budget min <= max
- After create/edit/delete: snackbar + navigate/refresh predictably
- Do NOT expose AI fields (`AiTags`, `AiProfile`) — Sprint 4

---

## Sprint 3 — Property Management `[Planned]`

**Goal:** Agents can create and manage property listings.
**Depends on:** Sprint 2 complete.

| # | Task | Label |
|---|---|---|
| 3.1 | Buyer schema + API contract foundation | `backend` |
| 3.2 | POST `/properties` — create property | `backend` |
| 3.3 | GET `/properties` — paginated list, filterable by `PropertyStatus` | `backend` |
| 3.4 | GET `/properties/{id}` + PUT `/properties/{id}` | `backend` |
| 3.5 | PATCH `/properties/{id}/status` — status transitions (Available → UnderContract → Sold) | `backend` |
| 3.6 | DELETE `/properties/{id}` — soft delete | `backend` |
| 3.7 | Property models/service + navigation entry | `frontend` |
| 3.8 | Property list page — MudDataGrid, status filter chips | `frontend` |
| 3.9 | Property create/edit/detail UI | `frontend` |

---

## Sprint 4 — AI Tagging via Gemini `[Planned]`

**Goal:** Gemini analyzes buyer/property descriptions and assigns semantic tags. Core of Phase 1.
**Depends on:** Sprint 2 and Sprint 3 complete.

| # | Task | Label |
|---|---|---|
| 4.1 | `GeminiTaggingService` — implements `IAITaggingService` using Google Generative AI SDK | `backend` |
| 4.2 | `TagBuyerCommand` — sends buyer description to Gemini, persists tags on `Buyer` | `backend` |
| 4.3 | `TagPropertyCommand` — sends property description to Gemini, persists tags on `Property` | `backend` |
| 4.4 | Auto-trigger tagging on Buyer/Property create and update | `backend` |
| 4.5 | Exponential backoff retry for Gemini 429 rate-limit errors | `backend` |
| 4.6 | Debug endpoints: GET `/buyers/{id}/tags`, GET `/properties/{id}/tags` | `backend` |

> Tags stored as JSON column on entity. Each tag: `name` + `weight` (float). Gemini prompt must return structured JSON.

---

## Sprint 5 — Matching Engine `[Planned]`

**Goal:** System proposes which buyers match which properties based on AI tags.
**Depends on:** Sprint 4 complete.

| # | Task | Label |
|---|---|---|
| 5.1 | `MatchingService` — weighted Jaccard similarity on tag sets | `backend` |
| 5.2 | GET `/properties/{id}/matches` — ranked buyer list with score | `backend` |
| 5.3 | GET `/buyers/{id}/matches` — ranked property list with score | `backend` |
| 5.4 | POST `/match-reports` — agent logs `BuyerReaction` (Interested, NotInterested, Visited, Offered) | `backend` |
| 5.5 | GET `/match-reports` — list for tenant | `backend` |
| 5.6 | Feedback loop — batch reaction history to Gemini to refine tags | `backend` |
| 5.7 | Match page for property — ranked buyer list + reaction action | `frontend` |
| 5.8 | Match page for buyer — ranked property list | `frontend` |
| 5.9 | Log reaction form | `frontend` |
| 5.10 | Match reports list — AgencyManager view | `frontend` |

> Match threshold for auto-notification is configurable in `appsettings.json`.

---

## Sprint 6 — In-App Notifications `[Planned]`

**Goal:** Users receive in-app notifications for relevant events. Completes Phase 1.
**Depends on:** Sprint 5 complete.

| # | Task | Label |
|---|---|---|
| 6.1 | `NotificationService` — creates `InAppNotification` records | `backend` |
| 6.2 | GET `/notifications` — unread notifications for authenticated user | `backend` |
| 6.3 | PATCH `/notifications/{id}/read` — mark as read | `backend` |
| 6.4 | Auto-create notification when match score exceeds threshold | `backend` |
| 6.5 | Notification bell icon in app bar with unread badge | `frontend` |
| 6.6 | Notification dropdown panel — list with mark-as-read | `frontend` |
| 6.7 | Polling: GET `/notifications` every 60 seconds | `frontend` |

> **Phase 1 complete after this sprint.**

---

## Sprint 7 — FSBO Scrapers `[Planned]`

**Goal:** Workers project continuously scrapes 5 portals and stores raw listings.
**Depends on:** Sprint 0 complete (Workers infrastructure). Can run in parallel with S5/S6.

| # | Task | Label |
|---|---|---|
| 7.1 | `ScraperWorkerBase` — abstract base with `PeriodicTimer`, error handling, Serilog, `CancellationToken` | `backend` |
| 7.2 | `FsboLeadRepository` — persist scraped listings, duplicate detection by URL | `backend` |
| 7.3 | Scraper: **4zida.rs** | `backend` |
| 7.4 | Scraper: **halooglasi.com** | `backend` |
| 7.5 | Scraper: **nadjidom.com** | `backend` |
| 7.6 | Scraper: **kupujemprodajem.com** | `backend` |
| 7.7 | Scraper: **nekretnine.rs** | `backend` |
| 7.8 | Graceful degradation — scraper throws → log full context + continue, never crash worker | `backend` |

> Each portal = separate `IHostedService`. Scraping interval per scraper in `appsettings.json`.
> **Open architecture decision:** `FsboLead` has no `TenantId` — create an `architecture` issue at Sprint 7 start to decide: (a) `TenantFsboLead` junction table, or (b) filter via `InAppNotification` reference.

---

## Sprint 8 — FSBO Classification & Lead Notifications `[Planned]`

**Goal:** Gemini classifies listings as FSBO or agency-posted. Agencies are notified. Completes Phase 2.
**Depends on:** Sprint 7 complete, Sprint 6 complete (notification infrastructure).

| # | Task | Label |
|---|---|---|
| 8.1 | `FsboClassificationService` — batch classify scraped listings via Gemini | `backend` |
| 8.2 | Batch logic — collect N unclassified listings, one Gemini call | `backend` |
| 8.3 | On FSBO detection: `InAppNotification` for all active AgencyManagers across all tenants | `backend` |
| 8.4 | Email notification via MailKit — FSBO alert to AgencyManager email | `backend` |
| 8.5 | GET `/fsbo-leads` — paginated list filterable by `FsboLeadStatus` | `backend` |
| 8.6 | PATCH `/fsbo-leads/{id}/status` — New → Contacted → Converted / Dismissed | `backend` |
| 8.7 | FSBO Leads list page — MudDataGrid, status filter, urgency sorting | `frontend` |
| 8.8 | FSBO Lead detail page — listing info, portal link, status change | `frontend` |
| 8.9 | Email notification settings form for AgencyManager (SMTP config per tenant) | `frontend` |

> **Phase 2 complete after this sprint.**

---

## Sprint 9 — AgencyManager Dashboard `[Planned]`

**Goal:** AgencyManager has a unified overview of agency activity and KPIs.
**Depends on:** Sprint 8 complete.

| # | Task | Label |
|---|---|---|
| 9.1 | GET `/dashboard/summary` — KPIs: active buyers, properties, matches this week, new FSBO leads | `backend` |
| 9.2 | GET `/dashboard/agent-activity` — per-agent breakdown: entries created, reactions logged | `backend` |
| 9.3 | AgencyManager dashboard page — MudCard KPI tiles, recent activity feed | `frontend` |
| 9.4 | Agent management page — list agents, invite by email, deactivate | `frontend` |
| 9.5 | Agency settings page — name, contact info, notification preferences | `frontend` |

---

## Sprint 10 — Production Readiness `[Planned]`

**Goal:** Safe deployment to Windows Server. Nothing breaks in production.
**Depends on:** All previous sprints complete.

| # | Task | Label |
|---|---|---|
| 10.1 | Environment variable config for all secrets (connection string, JWT key, Gemini API key, SMTP) | `backend` |
| 10.2 | EF Core migration apply script + runbook for production deploys | `architecture` |
| 10.3 | IIS `web.config` for API (reverse proxy, HTTPS redirect) | `backend` |
| 10.4 | Blazor WASM publish as static files under IIS | `backend` |
| 10.5 | Serilog file sink for production (log rotation, 30-day retention) | `backend` |
| 10.6 | Database index audit — N+1 review, missing indexes | `backend` |
| 10.7 | Health check endpoint — verify DB connection | `backend` |
| 10.8 | Security audit — CORS, HTTPS enforce, security headers, no secrets in code | `backend` |
