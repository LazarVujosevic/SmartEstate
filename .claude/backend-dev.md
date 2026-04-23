# Backend Developer — Role Instructions

> Read CLAUDE.md and `.claude/sprint-plans.md` in full before doing anything.
> Update this file and CLAUDE.md when you discover important implementation details or constraints.
> **⚠️ Do NOT commit changes to `.claude/sprint-plans.md` — that file is maintained exclusively by Lead Dev.**

---

## Your Role

You are the **Backend Developer** for SmartEstate.  
You implement .NET 10 API, Application, Domain, Infrastructure, and Workers code.  
You pick up GitHub Issues labeled `backend`, implement them, and submit Pull Requests.

---

## Workflow — Every Session

1. **Read CLAUDE.md** (mandatory, every session)
2. **Read this file** (mandatory, every session)
3. Check GitHub Issues with label `backend` that are open and not assigned to a PR
4. Pick the highest-priority open issue (or the one explicitly assigned to you)
5. Implement the feature following the conventions below
6. Create a Pull Request referencing the issue (`Closes #<issue-number>`)
7. **Do not start the next issue** until your open PR is reviewed and merged
8. After merge: update CLAUDE.md and this file with any new discoveries

---

## Solution Projects

| Project | Purpose |
|---|---|
| `SmartEstate.Domain` | Entities, value objects, domain events, enums. Zero external dependencies. |
| `SmartEstate.Application` | CQRS with MediatR, interfaces, DTOs, FluentValidation, business logic. Depends only on Domain. |
| `SmartEstate.Infrastructure` | EF Core DbContext, repositories, Gemini AI service, MailKit email, HtmlAgilityPack scrapers. |
| `SmartEstate.API` | ASP.NET Core controllers, middleware (tenant resolution, JWT), Swagger. |
| `SmartEstate.Workers` | IHostedService workers for Lead-Gen scraping. Separate deployable. |

---

## Key Patterns & Conventions

### CQRS with MediatR
Every feature lives in `Application/Features/{FeatureName}/`:
```
Features/
  Buyers/
    Commands/
      CreateBuyer/
        CreateBuyerCommand.cs      ← record implementing IRequest<Result<BuyerDto>>
        CreateBuyerCommandHandler.cs
        CreateBuyerCommandValidator.cs  ← FluentValidation
    Queries/
      GetBuyer/
        GetBuyerQuery.cs
        GetBuyerQueryHandler.cs
    DTOs/
      BuyerDto.cs
```

Controllers only dispatch:
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateBuyerCommand command, CancellationToken ct)
    => Ok(await _mediator.Send(command, ct));
```

### Result Pattern
Use `ErrorOr` (NuGet: `ErrorOr`) for all command/query results:
```csharp
// Handler returns:
ErrorOr<BuyerDto>

// Controller unwraps:
return result.Match(
    buyer => Ok(ApiResponse<BuyerDto>.Success(buyer)),
    errors => Problem(errors)
);
```

### Multi-Tenancy
All tenant-scoped entities:
```csharp
public class Buyer : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    // ...
}
```

`AppDbContext` receives `ITenantContext` and applies global query filters:
```csharp
modelBuilder.Entity<Buyer>().HasQueryFilter(b => b.TenantId == _tenantContext.TenantId);
```

**Never query tenant-scoped entities without the filter.** Only Administrator bypasses (using `.IgnoreQueryFilters()`).

### Entity Framework Core
- Use `IRepository<T>` interface in Application, implement `Repository<T>` in Infrastructure
- **No raw SQL** unless absolutely necessary and reviewed
- Always use `AsNoTracking()` for read-only queries
- Add migrations via: `dotnet ef migrations add <Name> --project SmartEstate.Infrastructure --startup-project SmartEstate.API`

### Authentication & Authorization
```csharp
[Authorize(Roles = "Agent,AgencyManager")]  // use roles from AppRoles constants
[HttpGet("buyers")]
```

Tenant resolution middleware (`TenantMiddleware`) runs before controllers and sets `ITenantContext` from JWT claim `tenant_id`.

### Gemini AI Integration
- Use `Google.AI.Generative` SDK (or REST if SDK unavailable)
- Service interface in Application: `IAITaggingService`
- Implementation in Infrastructure: `GeminiTaggingService`
- API key from configuration: `Gemini:ApiKey`
- **Batch requests** — never call Gemini in a per-entity loop; collect and batch
- Handle `429 Too Many Requests` gracefully with exponential backoff

### Email (MailKit)
- Service interface in Application: `IEmailService`
- Implementation in Infrastructure: `MailKitEmailService`
- Config: `Email:SmtpHost`, `Email:SmtpPort`, `Email:Username`, `Email:Password`, `Email:FromAddress`

### Logging
Use Serilog structured logging. Always include relevant context:
```csharp
_logger.LogInformation("Buyer {BuyerId} matched to property {PropertyId} with score {Score}", 
    buyer.Id, property.Id, score);
```

### Workers (Lead-Gen Scrapers)
Each portal has its own scraper service:
```
Workers/
  Scrapers/
    FourZidaScraper.cs     ← implements IPortalScraper
    HaloOglasiScraper.cs
    ...
  LeadGenWorker.cs         ← orchestrates scrapers on schedule
```

Use `PeriodicTimer` for scheduling. Scrapers must:
- Catch all exceptions (never crash the worker)
- Log failures with full context
- Use `CancellationToken` properly
- Store raw listing data before AI classification (for debugging/replay)

---

## API Response Convention

All endpoints return `ApiResponse<T>`:
```csharp
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public IEnumerable<string>? Errors { get; init; }
}
```

Standard HTTP status codes:
- `200 OK` — success with data
- `201 Created` — resource created (return `ApiResponse<T>` with new resource)
- `400 Bad Request` — validation errors (include `Errors` list)
- `401 Unauthorized` — not authenticated
- `403 Forbidden` — authenticated but wrong role/tenant
- `404 Not Found` — resource not found
- `500 Internal Server Error` — unexpected (log it, return generic message)

---

## NuGet Packages (Pre-approved)

| Package | Purpose |
|---|---|
| `MediatR` | CQRS |
| `FluentValidation.AspNetCore` | Validation |
| `ErrorOr` | Result pattern |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity |
| `Microsoft.EntityFrameworkCore.Design` | EF Core tools |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL EF provider |
| `Serilog.AspNetCore` | Logging |
| `Serilog.Sinks.Console` | Logging sink |
| `MailKit` | Email |
| `HtmlAgilityPack` | Web scraping |
| `Swashbuckle.AspNetCore` | Swagger |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT |

For any new package, check with Lead Dev first (create an `architecture` issue).

---

## Pull Request Guidelines

PR title format: `[Backend] <Brief description of what was implemented>`

PR description must include:
- `Closes #<issue-number>`
- What was implemented
- Any deviations from the issue spec and why
- Migration instructions if a new migration was added
- Any follow-up issues needed

---

## Important Rules

1. **Read CLAUDE.md first** — every session, no exceptions
2. **One PR per issue** — don't bundle unrelated changes
3. **Never commit secrets** — use `appsettings.Development.json` (gitignored) for local secrets
4. **Test your code locally** before creating a PR — at minimum run the API and verify endpoints work
5. **Update CLAUDE.md** if you discover anything that future sessions need to know
6. **Wait for PR review** before starting the next issue

---

## Implementation Notes & Discoveries

### NuGet Version Constraints
- `Serilog.Sinks.File` must be **`7.0.0`** — `Serilog.AspNetCore 10.0.0` requires `>= 7.0.0` transitively; specifying 6.0.0 causes NU1605 downgrade error that fails the build
- `Microsoft.EntityFrameworkCore.Design` must be added to **both** `SmartEstate.Infrastructure` (already there) **and** `SmartEstate.API` (startup project) — EF Core CLI tools check the startup project
- `System.IdentityModel.Tokens.Jwt 8.17.0` must be explicitly added to Infrastructure — NOT transitively available from `Microsoft.AspNetCore.App` framework reference even though `JwtBearer` is in the framework

### EF Core Migrations
- Install tool once: `dotnet tool install --global dotnet-ef`
- Generate migration: `dotnet ef migrations add <Name> --project src/SmartEstate.Infrastructure --startup-project src/SmartEstate.API --output-dir Persistence/Migrations`
- Apply to DB: `dotnet ef database update --project src/SmartEstate.Infrastructure --startup-project src/SmartEstate.API`
- The `[ERR] Failed executing DbCommand SELECT from __EFMigrationsHistory` log on first `database update` is **normal** — EF Core checks if the history table exists, gets an error (it doesn't), then creates it. Confirmed by "Done" at the end.
- Connection string format for Npgsql: `Host=localhost;Port=5432;Database=smartestate;Username=smartestate;Password=<see .env>`

### AppDbContext — Global Query Filters (CRITICAL)
- **DO NOT** extract `tenantContext.TenantId` as a local variable in `OnModelCreating`:
  ```csharp
  // BUG — captures value at model-build time, stale for all subsequent requests
  var tenantId = tenantContext.TenantId;
  builder.Entity<Buyer>().HasQueryFilter(e => e.TenantId == tenantId.Value);
  ```
- **CORRECT** — reference the primary-constructor captured field directly:
  ```csharp
  builder.Entity<Buyer>().HasQueryFilter(e =>
      !tenantContext.TenantId.HasValue || e.TenantId == tenantContext.TenantId.Value);
  ```
  EF Core re-evaluates the field reference per-query. When `TenantId` is null (Administrator or no HttpContext), the filter is bypassed — all records visible.

### DataSeeder
- Location: `SmartEstate.Infrastructure/Persistence/DataSeeder.cs`
- Registered as `AddScoped<DataSeeder>()` in `Infrastructure/DependencyInjection.cs`
- Invoked in `API/Program.cs` via `CreateAsyncScope()` before `app.Run()`
- Seeds: all three roles (`AppRoles.All`), then Administrator user from `ADMIN_EMAIL` / `ADMIN_PASSWORD` env vars
- If env vars are missing → logs warning, skips gracefully (no crash)
- Admin user always has `TenantId = null`

### Serilog
- API: `UseSerilog()` on `builder.Host` (IHostBuilder extension)
- Workers: `AddSerilog()` on `builder.Services` (HostApplicationBuilder doesn't expose IHostBuilder directly)
- `SensitivePropertyDestructuringPolicy` at `SmartEstate.Infrastructure/Logging/` — only applies to SmartEstate namespace types, redacts password/token/secret/apikey/authorization properties
- Log files: `logs/smartestate-.log` (API), `logs/smartestate-workers-.log` (Workers). The `logs/` directory is in `.gitignore`.

### Roles & AppRoles
- `AppRoles` constants are defined in `SmartEstate.Infrastructure.Identity` namespace, same file as `ApplicationRole`: `Infrastructure/Identity/ApplicationRole.cs`
- `AppRoles.All` is a string array: `[Administrator, AgencyManager, Agent]` — use it for bulk role seeding

### Authentication & JWT (Sprint 1)
- `IAuthService` interface in `Application/Common/Interfaces/` — implemented by `AuthService` in `Infrastructure/Identity/`; use this pattern whenever Application needs Identity concerns (`UserManager`, etc.)
- `JwtSettings` at `Infrastructure/Identity/JwtSettings.cs` — bound via `services.Configure<JwtSettings>(configuration.GetSection("Jwt"))`; injected as `IOptions<JwtSettings>`
- JWT claims written by `AuthService`: `sub` (userId), `email`, `role` (ClaimTypes.Role), `tenant_id` (AppClaims.TenantId — only if not Administrator), `jti`, `exp`
- `AppClaims.TenantId` constant = `"tenant_id"` — defined in `Application/Common/Constants/AppClaims.cs`; never use the string literal directly
- `JwtSecurityTokenHandler` should be `static readonly` in `AuthService` — thread-safe, no need to instantiate per-request
- If user has no roles assigned, return `Error.Unauthorized` — do not issue a token with empty role claim
- JWT `Secret` must be ≥ 32 chars — validated at startup, app throws `InvalidOperationException` if not
- API authentication must explicitly set JWT bearer as `DefaultAuthenticateScheme`, `DefaultChallengeScheme`, and `DefaultScheme`; Identity registers cookie schemes and can otherwise cause `[Authorize]` API endpoints to redirect to `/Account/Login` instead of accepting bearer tokens.
- API skips `UseHttpsRedirection()` in Development only to avoid interfering with alternate local profiles; Visual Studio HTTPS profiles remain the expected manual testing path. Keep HTTPS enforcement for non-Development environments.

### Multi-Tenancy (Sprint 1)
- `TenantContext` (`Infrastructure/Services/TenantContext.cs`) is a **settable POCO** — `TenantId` and `IsAdministrator` are set by `TenantMiddleware` at request start; do NOT read from `IHttpContextAccessor` inside `TenantContext`
- **Double-registration required** in `DependencyInjection.cs`:
  ```csharp
  services.AddScoped<TenantContext>();
  services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
  ```
  Middleware resolves `TenantContext` (concrete) to set values; all other consumers resolve `ITenantContext` (interface). Both get the same scoped instance.
- `TenantMiddleware` at `Infrastructure/MultiTenancy/TenantMiddleware.cs` — registered after `UseAuthorization()` in `Program.cs`
- Middleware only sets `TenantId` when user is authenticated AND claim is present and valid Guid — silent skip otherwise

### Exception Handling (Sprint 1)
- `GlobalExceptionHandler` (`API/Common/GlobalExceptionHandler.cs`) implements `IExceptionHandler`
- Catches `FluentValidation.ValidationException` → `400 Bad Request` with `ApiResponse.Fail("Validation failed.", errors)`
- Catches all other exceptions → `500 Internal Server Error` with `ApiResponse.Fail("An unexpected error occurred.")`
- Registered in `Program.cs`: `services.AddExceptionHandler<GlobalExceptionHandler>()` + `services.AddProblemDetails()`; activated with `app.UseExceptionHandler()`
- **Never use `ControllerBase.Problem()`** in error branches — it returns RFC 7807 `ProblemDetails`, not `ApiResponse`; use `StatusCode(500, ApiResponse.Fail(...))` instead
- `UseSerilogRequestLogging()` must be **before** `UseExceptionHandler()` in the middleware pipeline

### Admin Controllers (Sprint 1)
- Admin controllers live in `API/Controllers/`, route prefix `api/admin/...`, with `[Authorize(Roles = AppRoles.Administrator)]` at controller class level
- Shared `private IActionResult MapErrors(List<Error> errors)` helper on each admin controller — maps `NotFound → 404`, `Conflict → 409`, `Validation → 400` (all messages in `ApiResponse.Errors[]`), `other → 500`
- Use `CreatedAtAction(nameof(ActionName), new { id = dto.Id }, body)` for 201 responses — when a GET endpoint is added later, update the action name reference
- Duplicate-check queries (e.g. tenant name uniqueness) use `==` which is case-sensitive in PostgreSQL — for user-facing uniqueness, consider `EF.Functions.ILike` or normalizing to lowercase
- Request body records (route param + body split) go in `API/Models/Requests/` — one file per record

### Inactive Tenant & Cache (Sprint 1)
- `InactiveTenantMiddleware` (`Infrastructure/MultiTenancy/`) runs after `TenantMiddleware`; skips check when `TenantId is null` (Administrator/unauthenticated) or path is `/health`/`/ping`
- Cache key pattern: `"tenant_active_{tenantId}"` — `IMemoryCache` TTL 60 seconds; scoped services injected via `InvokeAsync` parameters (not constructor — middleware constructors are effectively singleton)
- `ITenantCache` interface in `Application/Common/Interfaces/` — single method `void InvalidateTenant(Guid tenantId)`; implemented by `TenantCache` in `Infrastructure/MultiTenancy/`; inject into handlers that toggle `Tenant.IsActive` to keep cache consistent
- `services.AddMemoryCache()` is in `Infrastructure/DependencyInjection.cs`

### User Management (Sprint 1)
- `IUserManagementService` in `Application/Common/Interfaces/` — implemented by `UserManagementService` in `Infrastructure/Identity/`; follows same pattern as `IAuthService`
- Identity `CreateAsync` errors are returned as a `List<Error>` (one `Error.Validation` per Identity error) — do NOT join into a single string; controller `MapErrors` collects all descriptions into `ApiResponse.Errors[]`
- Always check email uniqueness with `FindByEmailAsync` **before** calling `CreateAsync` to distinguish 409 (duplicate) from 400 (password policy)
- `Tenant.Plan` is `string?` — added via migration `AddTenantPlan` (20260423...); validator enforces `NotEmpty()` at the API boundary so stored value is never empty string

### Buyer CRUD (Sprint 2)
- `ISoftDeletable` interface at `Domain/Common/ISoftDeletable.cs` — `Buyer` implements it; `Property` will follow in Sprint 3
- `AppDbContext`: EF Core allows only **one** `HasQueryFilter` per entity — `Buyer` uses an inline combined lambda `(tenant filter) && !e.IsDeleted`; do NOT call `ApplyTenantFilter<Buyer>` and then override with a second call
- `AssignedAgentId` extracted in controller via `User.FindFirstValue(ClaimTypes.NameIdentifier)` — passed into command, never from request body
- `PreferredLocations` migration column requires `defaultValueSql: "ARRAY[]::text[]"` — PostgreSQL rejects NOT NULL column addition on a table with existing rows without a default
- `DeleteBuyerCommand` returns `ErrorOr<Deleted>` with `Result.Deleted` — matches ErrorOr library pattern for void success results
- `BuyerDto` must NOT expose `TenantId` — internal multi-tenancy concern, never crosses API boundary
- Stacked PRs (when multiple issues touch same file): set base branch to previous feature branch; Lead Dev changes base to `main` before merging each one in order
- `GET /api/buyers` search is LINQ `.ToLower().Contains()` — translated to PostgreSQL `ILIKE`-equivalent by Npgsql; no manual `EF.Functions.ILike` needed for simple contains

### Docker
- Credentials are in `.env` (gitignored). Copy `.env.example` → `.env` before first run.
- pgAdmin auto-connects to PostgreSQL via `docker/pgadmin/servers.json` (mounted read-only)
- ASP.NET Identity tables are prefixed `AspNet` by convention (e.g. `AspNetUsers`, `AspNetRoles`) — keep this for consistency
- Blazor WASM expects API at the base URL configured in `wwwroot/appsettings.json` — coordinate with Frontend Dev
