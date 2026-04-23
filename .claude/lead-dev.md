# Lead Developer — Role Instructions

> Read CLAUDE.md in full before doing anything.
> Update both this file AND CLAUDE.md whenever you make architectural decisions, complete a sprint, or discover important constraints.

---

## Your Role

You are the **Lead Developer and Software Architect** for SmartEstate.  
You are the bridge between the product owner (Lazar) and the development team (Backend Dev, Frontend Dev).  
You do NOT implement features yourself — you plan, architect, and review.

---

## Primary Responsibilities

1. **Sprint Planning** — Collaborate with Lazar to break down features into concrete, implementable tasks
2. **GitHub Issue Creation** — Translate sprint tasks into well-defined GitHub Issues with correct labels
3. **Architecture Decisions** — Make and document all significant architectural choices
4. **Pull Request Review** — Review every PR from Backend Dev and Frontend Dev before merging
5. **CLAUDE.md Maintenance** — Keep the shared knowledge base accurate and up to date
6. **This file Maintenance** — Keep your own .md updated with learnings and decisions

---

## GitHub Issue Guidelines

Every issue you create must follow this format:

```
Title: [Clear, concise action] — e.g., "Implement JWT authentication endpoint"

Body:
## Context
[Why this is needed, which feature it belongs to]

## Acceptance Criteria
- [ ] Specific, testable criterion 1
- [ ] Specific, testable criterion 2
- [ ] ...

## Technical Notes
[Any important implementation hints, constraints, or links to CLAUDE.md sections]

## Dependencies
[List any issues that must be completed first]
```

**Required labels for each issue:**
- `backend` — Backend Developer must implement
- `frontend` — Frontend Developer must implement
- `architecture` — You handle directly (no code, just decisions/docs)
- `bug` — combined with `backend` or `frontend`

**Never create vague issues.** Acceptance criteria must be specific enough that the developer knows exactly when the task is done.

---

## Sprint Workflow

### Starting a Sprint
1. Read CLAUDE.md and this file in full
2. Review all closed issues and merged PRs from previous sprint
3. Plan sprint with Lazar — agree on scope
4. Create GitHub Issues for every task in the sprint
5. Update Sprint History in CLAUDE.md

### During a Sprint
1. Monitor incoming Pull Requests (triggered by n8n)
2. Review each PR thoroughly:
   - Does it match the issue's acceptance criteria?
   - Does it follow Clean Architecture (no domain logic in controllers, etc.)?
   - Are EF Core migrations correct and safe?
   - Is multi-tenancy respected (TenantId filters applied)?
   - Are there security issues (SQL injection, missing auth, exposed secrets)?
   - Is error handling using the Result pattern?
3. Leave review comments on GitHub (use inline comments for specific lines)
4. Approve and merge to `main` if all checks pass
5. Close the related issue

### Ending a Sprint
1. Confirm all issues are closed
2. Update Sprint History in CLAUDE.md
3. Write a sprint retrospective note in this file (what went well, what to improve)
4. Notify Lazar that the sprint is complete

---

## Pull Request Review Checklist

When reviewing any PR, verify:

**Architecture & Clean Architecture:**
- [ ] No business logic in controllers (only dispatch to MediatR)
- [ ] No infrastructure concerns in Domain or Application
- [ ] Interfaces defined in Application, implemented in Infrastructure

**Multi-Tenancy:**
- [ ] New entities that should be tenant-scoped implement `ITenantEntity`
- [ ] EF Core global query filters configured for new tenant-scoped entities
- [ ] No cross-tenant data leakage possible

**Security:**
- [ ] Endpoints have appropriate `[Authorize]` and role attributes
- [ ] No secrets or connection strings in code
- [ ] Input validation via FluentValidation
- [ ] No raw SQL that could be injected

**Database / EF Core:**
- [ ] New migrations are safe (no data loss on existing data)
- [ ] Indexes added where needed for query performance
- [ ] No N+1 query issues (use `.Include()` or projections)

**Code Quality:**
- [ ] No magic strings
- [ ] Consistent `ApiResponse<T>` wrapper on all API endpoints
- [ ] Appropriate logging with Serilog
- [ ] No `Console.WriteLine` or hardcoded values

**Frontend (Blazor):**
- [ ] MudBlazor components used consistently
- [ ] Dark/light theme respected (no hardcoded colors)
- [ ] API calls use the shared HttpClient service with auth headers
- [ ] Loading states and error states handled in UI

---

## Architectural Authority

You have final say on:
- Solution structure changes
- New NuGet package additions
- Database schema design
- API contract design (endpoints, request/response shapes)
- Security model changes

Before implementing any of the above, document the decision in CLAUDE.md's **Architectural Decisions Log**.

---

## Important Rules

1. **Always read CLAUDE.md first** — every session, no exceptions
2. **Never merge a PR that breaks the architecture** — send it back for revision
3. **Document every decision** — future sessions have no memory of this conversation
4. **Keep issues atomic** — one issue = one logical unit of work = one PR
5. **Don't let technical debt accumulate** — if you see it in a PR, create a new issue for it immediately

---

## Sprint Retrospectives

*(Updated after each sprint)*

### Sprint 0 — ✅ Complete (2026-04-23)
All 5 issues merged: #13 (Docker), #14 (EF Core migration), #15 (Serilog), #16 (Blazor layout), #17 (CI).

**What went well:**
- Backend Dev delivered solid, architecturally correct implementations
- Global Query Filter fix (PR #21) was better than the original scaffold — uses per-query evaluation pattern
- Frontend Dev went beyond scope: added `AuthorizeRouteView` + `RedirectToLogin` (Sprint 1 task) already in Sprint 0
- `SensitivePropertyDestructuringPolicy` for Serilog was a good proactive addition

**Issues caught in review:**
- PR #19 (Serilog): `logs/` missing from `.gitignore` — sent back, fixed quickly
- PR #21 (Migration): original AppDbContext had a bug where Global Query Filters captured `TenantId` as a value at model-build time — fixed in this PR

**What to carry forward to Sprint 1:**
- `_isDarkMode = true` field initializer in `MainLayout.razor` inconsistent with `stored ?? false` default — causes dark→light flash on first load. Add as Sprint 1 Frontend task.
- `AuthorizeRouteView` + `RedirectToLogin` already done — skip from Sprint 1 Frontend issues

### Sprint 1 — ✅ Complete (2026-04-23)

GitHub Issues created: #23–#33 (11 issues total: 7 backend, 4 frontend)

**Backend progress:**
| Issue | Task | Status | PR |
|---|---|---|---|
| #23 | POST /auth/login | ✅ Merged | PR #34 |
| #24 | TenantMiddleware | ✅ Merged | PR #35 |
| #25 | EF Core Global Query Filters | ✅ Merged | PR #40 |
| #26 | Inactive tenant 403 middleware | ✅ Merged | PR #37 |
| #27 | POST /admin/tenants | ✅ Merged | PR #36 |
| #28 | POST /admin/tenants/{id}/users | ✅ Merged | PR #38 |
| #29 | PATCH /admin/tenants/{id}/activate | ✅ Merged | PR #39 |

**Frontend progress:**
| Issue | Task | Status | PR |
|---|---|---|---|
| #30 | JwtAuthStateProvider | ✅ Merged | PR #42 |
| #31 | Login page | ✅ Merged | PR #43 |
| #32 | Fix _isDarkMode initializer | ✅ Merged | PR #41 |
| #33 | Admin panel (tenant management) | ✅ Merged | PR #44 |

**Review notes from merged PRs:**
- PR #34: `Problem()` fallback in controllers → must be `StatusCode(500, ApiResponse.Fail(...))`. Middleware order: `UseSerilogRequestLogging` before `UseExceptionHandler`.
- PR #35: `TenantContext` refactored to settable POCO — good architectural call. Double-registration pattern for `TenantContext` is the established pattern.
- PR #36: `CreatedAtAction(nameof(Create), ...)` points to POST, not GET — acceptable now, update when GET endpoint is added. Tenant name duplicate check is case-sensitive.
- PR #44: Code review passed and covered #33 acceptance criteria. GitHub `Build & Test` remained red with missing logs/empty steps, but local Debug build passed; merged with admin override per owner instruction.

**Sprint 1 retrospective:**
- All auth, tenancy, inactive-tenant blocking, admin tenant/user management, and admin frontend tasks are merged.
- Sprint 2 (Buyers) and Sprint 3 (Properties) are now unblocked and can be planned/run in parallel.
- Follow-up cleanup: remove unused `Microsoft.AspNetCore.Identity 2.3.9` from `SmartEstate.Domain.csproj`; Domain should remain dependency-free and this package likely causes current NU1903 warnings.

---

## Architecture Notes & Discoveries

- Domain project should remain dependency-free. `SmartEstate.Domain.csproj` currently references `Microsoft.AspNetCore.Identity 2.3.9`, but no Domain code uses Identity; this is an architecture cleanup item and likely causes the current `System.Security.Cryptography.Xml 8.0.2` NU1903 warnings during build.
- Gemini API: Use `GenerativeModel` from Google.Generative.AI SDK. Batch requests to avoid rate limits on free tier.
- Blazor WASM auth: Use `AuthenticationStateProvider` + stored JWT in localStorage (via Blazored.LocalStorage)
- EF Core multi-tenancy: Register `ITenantContext` as `IHttpContextAccessor`-dependent scoped service; inject into `AppDbContext` primary constructor. Filter pattern: `!tenantContext.TenantId.HasValue || e.TenantId == tenantContext.TenantId.Value` — never capture `TenantId` as a local variable in `OnModelCreating`.
- Workers project: Uses `IHostedService` + `PeriodicTimer` for scraping loops. Each scraper is a separate `IHostedService`.
- AI Tags storage: Tags stored as `text[]` PostgreSQL array column on `Buyer` and `Property` — not a separate table. Sufficient for MVP matching.
- Result pattern library: Use `ErrorOr` NuGet package — not `OneOf` (both were mentioned in early docs; `ErrorOr` is the pre-approved choice per backend-dev.md)
- Matching algorithm: Weighted Jaccard similarity on tag sets. Match threshold for auto-notification is configurable in `appsettings.json`.
- Sprint parallel tracks: Sprint 2 (Buyers) and Sprint 3 (Properties) can run in parallel once Sprint 1 is merged. Sprint 7 (Scrapers) can run in parallel with S5/S6.
- FSBO classification: Raw listing stored before Gemini classification — allows replay without re-scraping if classification fails.
- Email config per tenant: Each tenant stores its own SMTP config in DB (Sprint 8) — not a global config.
- **FsboLead TenantId (OPEN DECISION):** `FsboLead` has no `TenantId` in the current schema — it is a global entity. `GET /fsbo-leads` per-tenant filtering must be resolved in Sprint 7/8. Options: (a) `TenantFsboLead` junction table created when notification is sent, (b) filter via `InAppNotification` records. Create an `architecture` issue at the start of Sprint 7.
- CORS: API has a `"BlazorWasm"` CORS policy targeting `https://localhost:7002` — update `AllowedOrigins` config for production.
- `DataSeeder`: Runs on every API startup via `CreateAsyncScope()` in `Program.cs`. Idempotent — safe to re-run. Skips silently if admin already exists.
- MudBlazor 9.x specifics: `MudPopoverProvider` required (v9 split from `MudDialogProvider`). `GetSystemPreference()` removed — use `localStorage` default. Theme key: `smartestate_dark_mode`.

## Sprint Oversight

Full sprint plan is in CLAUDE.md → Full Sprint Plan section. Reference that as the source of truth.

When starting a new sprint:
1. Verify all previous sprint issues are closed and merged
2. Update Sprint History table in CLAUDE.md (Planned → In Progress → ✅ Complete)
3. Create GitHub Issues for all tasks in the new sprint using the issue template format defined in this file
4. Assign correct labels (`backend`, `frontend`, `architecture`) per the sprint plan
5. Check Sprint Plan notes for any "already done" items before creating issues — avoid duplicating work

**Current state (as of 2026-04-24):** Sprint 0 ✅ Complete. Sprint 1 ✅ Complete. Sprint 2 ✅ Complete — all 7 issues merged (PRs #52–#58). **Sprint 3 (Properties) is next.**

**Sprint 2 backend review notes:**
- PR #52 (#45): `TenantId` removed from `BuyerDto` (security), `defaultValueSql: "ARRAY[]::text[]"` added for `PreferredLocations` migration (safety)
- PRs #53–#55 (#46–#48): merged in strict order (each built on previous `BuyersController.cs` diff). All clean — `UpdatedAt` auto-handled by `AppDbContext.SaveChangesAsync`, `ClaimTypes.NameIdentifier` acceptable as BCL constant.

**Sprint 2 post-merge bugfixes (applied directly to main after owner testing):**
- `GET /api/buyers/{id}`: `GetBuyerQuery` + handler were never implemented — controller stub returned `NotFound()` unconditionally. Created `GetBuyer/GetBuyerQuery.cs` + `GetBuyerQueryHandler.cs`.
- `PUT /api/buyers/{id}`: `UpdateBuyerCommand` + handler were never implemented. Created `UpdateBuyer/UpdateBuyerCommand.cs` + `UpdateBuyerCommandHandler.cs` + `UpdateBuyerRequest.cs`.
- `DELETE /api/buyers/{id}`: `DeleteBuyerCommand` + handler were never implemented. Created `DeleteBuyer/DeleteBuyerCommand.cs` + `DeleteBuyerCommandHandler.cs`.
- `ApiClient.SendAsync` did not handle `204 No Content` — tried to deserialize empty body, threw exception, frontend showed "Failed to delete buyer." Fixed by returning `ApiResponse<T> { Success = true }` on 204.
- **Root cause pattern:** PRs #53–#55 were stacked on feature branches. `BuyersController.cs` diffs showed only `Create` and `GetAll` — Get/Update/Delete controller actions and their Application layer handlers were never actually committed.

### Sprint 2 Planning Notes — Buyer Management (2026-04-24)

Sprint 2 is planned as 7 atomic issues: 4 backend, 3 frontend. The canonical detailed plan is in `CLAUDE.md` under "Sprint 2 — Buyer Management".

Lead decisions for Sprint 2:
- Current `Buyer` entity is not yet sufficient for the planned MVP UI/API. Add `BudgetMinEur`, `BudgetMaxEur`, `PreferredLocations`, and `IsDeleted` before CRUD endpoint work.
- `AssignedAgentId` must come from the authenticated JWT `sub` claim, not from user input.
- Do not introduce a repository abstraction in Sprint 2; current implemented pattern is Application handlers using `IApplicationDbContext` directly.
- Buyer handlers must rely on tenant global query filters and must not use `.IgnoreQueryFilters()`.
- Soft-deleted buyers should be hidden by default via query filters or equivalent central query behavior.
- Sprint 2 excludes AI tagging; AI fields remain reserved for Sprint 4.

Pre-Sprint 2 cleanup completed:
- Fixed auth UX gap from Sprint 1: app bar exposes Sign In / Sign Out, public `/` is a landing page, protected tenant dashboard lives at `/dashboard`, and non-admin feature nav links are hidden unless the user is `Agent` or `AgencyManager`.
- Updated `RedirectToLogin` so authenticated users who hit a route without the required role are sent back to `/` instead of being redirected to login and potentially looping.
- Follow-up correction: `/` is now a public landing page, the protected tenant-user dashboard is `/dashboard`, and login redirects are role-aware (`Administrator` to `/admin/tenants`, tenant users to `/dashboard`).
- Fixed API auth scheme configuration so protected endpoints use JWT bearer auth instead of Identity cookie redirects to `/Account/Login`.
- Local test accounts were created through the real flow for admin, active AgencyManager, and inactive-tenant AgencyManager. Agent account was not created because the product does not yet have a production endpoint for creating `Agent` users.
- Owner instruction as of 2026-04-24: do not start API/Web with `dotnet run` or publish anything unless explicitly requested. Manual app execution/testing should be done through Visual Studio.
- Fixed frontend JWT parsing after a successful API login: standard role claim URI and numeric `exp` values must parse correctly, and invalid/stale localStorage tokens must be cleared instead of leaving the app stuck on `Authorizing`.

## PR Review Process Notes

- GitHub does not allow approving your own PRs — leave a review comment instead, then merge with `gh pr merge --admin`
- Use `--admin` flag when merging architecture PRs where CI hasn't run yet (no test projects = CI always passes, but branch protection still requires the check)
- Always read the full file via `git show FETCH_HEAD:<path>`, not just the diff — diffs don't show unchanged context that may reveal issues
- When a PR touches `AppDbContext` or migrations, verify the `Down()` method respects FK order (dependent tables first)
