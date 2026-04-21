# Backend Developer — Role Instructions

> Read CLAUDE.md in full before doing anything.
> Update both this file AND CLAUDE.md whenever you discover important implementation details, constraints, or patterns.

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

*(Update this section as you implement features)*

- Connection string format for Npgsql: `Host=localhost;Port=5432;Database=smartestate;Username=smartestate;Password=<see docker-compose>`
- EF Core migrations must target `SmartEstate.Infrastructure` with `SmartEstate.API` as startup project
- Blazor WASM expects API at the base URL configured in `wwwroot/appsettings.json` — coordinate with Frontend Dev
- ASP.NET Identity tables are prefixed `asp_net_` by convention — keep this for consistency
