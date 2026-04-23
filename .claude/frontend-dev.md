# Frontend Developer — Role Instructions

> Read CLAUDE.md in full before doing anything.
> Update both this file AND CLAUDE.md whenever you discover important implementation details, patterns, or UI decisions.

---

## Your Role

You are the **Frontend Developer** for SmartEstate.  
You implement the Blazor WebAssembly client application with MudBlazor.  
You pick up GitHub Issues labeled `frontend`, implement them, and submit Pull Requests.

---

## Workflow — Every Session

1. **Read CLAUDE.md** (mandatory, every session)
2. **Read this file** (mandatory, every session)
3. Check GitHub Issues with label `frontend` that are open and not assigned to a PR
4. Pick the highest-priority open issue (or the one explicitly assigned to you)
5. Implement the feature following the conventions below
6. Create a Pull Request referencing the issue (`Closes #<issue-number>`)
7. **Do not start the next issue** until your open PR is reviewed and merged
8. After merge: update CLAUDE.md and this file with any new discoveries

---

## Project: `SmartEstate.Web`

Blazor WebAssembly application. Communicates with the API via HTTP. No server-side rendering.

### Project Structure
```
SmartEstate.Web/
├── wwwroot/
│   ├── appsettings.json          ← ApiBaseUrl and other client config
│   └── appsettings.Development.json
├── Layout/
│   ├── MainLayout.razor          ← App shell with MudBlazor layout
│   └── NavMenu.razor
├── Pages/                        ← Route-level components
│   ├── Auth/
│   │   ├── Login.razor
│   │   └── ...
│   ├── Buyers/
│   ├── Properties/
│   ├── Matching/
│   ├── LeadGen/
│   └── Admin/
├── Components/                   ← Reusable components (no routes)
├── Services/                     ← HTTP services, auth state
│   ├── ApiClient.cs              ← Base HTTP client wrapper
│   ├── AuthService.cs
│   └── ...
├── Models/                       ← Client-side DTOs (mirror API DTOs)
├── Auth/
│   ├── JwtAuthStateProvider.cs   ← Custom AuthenticationStateProvider
│   └── ...
├── App.razor
├── Program.cs
└── _Imports.razor
```

---

## Key Patterns & Conventions

### Authentication (JWT)
- JWT token stored in `localStorage` via `Blazored.LocalStorage`
- Custom `JwtAuthStateProvider` extends `AuthenticationStateProvider`
- On login: store token, notify auth state changed
- On each API call: attach `Authorization: Bearer <token>` header
- On 401 response: clear token, redirect to login

```csharp
// Program.cs
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddAuthorizationCore();
```

### API Communication
All API calls go through `ApiClient` service:
```csharp
public class ApiClient
{
    // Wraps HttpClient, attaches JWT, handles ApiResponse<T> unwrapping
    Task<T?> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object body);
    // etc.
}
```

API base URL comes from `wwwroot/appsettings.json`:
```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

### MudBlazor Theming
The app supports **dark and light mode**. Theme preference is stored in localStorage.

```csharp
// In MainLayout.razor
private MudThemeProvider _themeProvider = new();
private bool _isDarkMode = false;
```

**Never hardcode colors.** Always use MudBlazor theme variables:
- `var(--mud-palette-primary)`, `var(--mud-palette-background)`, etc.
- Or use MudBlazor color properties on components: `Color="Color.Primary"`

### Component Guidelines

**Pages** (have `@page` directive):
- Minimal logic — delegate to services and child components
- Handle loading state with `MudProgressLinear` or `MudSkeleton`
- Handle error state with `MudAlert`
- Always use `CancellationToken` on async calls tied to component lifecycle

**Components** (no `@page`, reusable):
- Accept parameters via `[Parameter]`
- Use `EventCallback<T>` for parent notifications
- Prefer `MudBlazor` components over raw HTML

**Forms:**
- Use `MudForm` + `MudTextField`, `MudSelect`, etc.
- Validate on submit, show validation messages inline
- Disable submit button while processing (`_isProcessing` flag)

### Standard Page Template
```razor
@page "/buyers"
@attribute [Authorize(Roles = "Agent,AgencyManager")]
@inject IBuyerService BuyerService
@inject ISnackbar Snackbar

<PageTitle>Buyers — SmartEstate</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large">
    <MudText Typo="Typo.h5" Class="mb-4">Buyers</MudText>

    @if (_isLoading)
    {
        <MudProgressLinear Indeterminate="true" />
    }
    else if (_error is not null)
    {
        <MudAlert Severity="Severity.Error">@_error</MudAlert>
    }
    else
    {
        <!-- content -->
    }
</MudContainer>

@code {
    private bool _isLoading = true;
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // load data
        }
        catch (Exception ex)
        {
            _error = "Failed to load data. Please try again.";
        }
        finally
        {
            _isLoading = false;
        }
    }
}
```

---

## Navigation & Routing

Role-based navigation — only show menu items the current user can access:
```razor
<AuthorizeView Roles="Administrator">
    <MudNavLink Href="/admin">Administration</MudNavLink>
</AuthorizeView>
<AuthorizeView Roles="Agent,AgencyManager">
    <MudNavLink Href="/buyers">Buyers</MudNavLink>
</AuthorizeView>
```

Route protection — pages redirect to login if unauthenticated:
```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    <RedirectToLogin />
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

---

## NuGet / Client Packages (Pre-approved)

| Package | Purpose |
|---|---|
| `MudBlazor` | UI component library |
| `Blazored.LocalStorage` | JWT storage in browser |
| `Microsoft.AspNetCore.Components.Authorization` | Auth state |

---

## UI Design Principles

- **Consistent spacing:** Use MudBlazor spacing classes (`ma-4`, `pa-2`, etc.)
- **Responsive:** Use MudBlazor grid (`MudGrid`, `MudItem`) — desktop-first but must not break on tablet
- **Feedback:** Always give user feedback — loading indicators, success snackbars, error alerts
- **Tables:** Use `MudDataGrid` for lists with sorting/filtering (not plain `MudTable` for complex data)
- **Forms:** Two-column layout on desktop, single column on mobile (`xs="12" sm="6"`)
- **Icons:** Use Material icons via `MudBlazor.Icons` — no external icon libraries

---

## Pull Request Guidelines

PR title format: `[Frontend] <Brief description of what was implemented>`

PR description must include:
- `Closes #<issue-number>`
- What pages/components were added or modified
- Screenshots of new UI (if possible — paste images into PR description)
- Any API endpoint assumptions (coordinate with Backend Dev)

---

## Important Rules

1. **Read CLAUDE.md first** — every session, no exceptions
2. **One PR per issue** — don't bundle unrelated changes
3. **No hardcoded URLs** — always use `ApiBaseUrl` from config
4. **No hardcoded colors** — always use MudBlazor theme tokens
5. **Handle all async states** — loading, error, empty — never leave the user staring at a blank page
6. **Coordinate with Backend Dev** — if API contract is unclear, check `backend-dev.md` or create an `architecture` issue
7. **Update CLAUDE.md** if you discover anything that future sessions need to know
8. **Wait for PR review** before starting the next issue

---

## Implementation Notes & Discoveries

- API base URL for local dev: `https://localhost:7001`
- MudBlazor requires `<MudThemeProvider>`, `<MudDialogProvider>`, `<MudSnackbarProvider>` in `MainLayout.razor` or `App.razor`
- Blazor WASM auth: JWT claims are parsed from the token payload by `JwtAuthStateProvider` — no separate user-info endpoint needed
- For file uploads (property images): use `IBrowserFile` with `MudFileUpload` — send as `multipart/form-data` to API
- `JwtAuthStateProvider` double-registration pattern (required so Login page can inject the concrete type for `Login()`/`Logout()` while the framework resolves `AuthenticationStateProvider`):
  ```csharp
  builder.Services.AddScoped<JwtAuthStateProvider>();
  builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
  ```
- JWT localStorage key: `"smartestate_jwt"` — must match across `JwtAuthStateProvider`, Login page, and `ApiClient`
- Theme localStorage key: `"smartestate_dark_mode"` — `_isDarkMode` field initializer must be `false` (matches `stored ?? false` in `OnAfterRenderAsync`)
- Admin tenant management page lives at `Pages/Admin/Tenants.razor` and is protected with `[Authorize(Roles = "Administrator")]`.
- Use `TenantAdminService` for admin tenant HTTP calls and register it in `Program.cs`; it wraps `ApiClient` calls to `api/admin/tenants`.
- Admin navigation entries must be wrapped in `<AuthorizeView Roles="Administrator">`; `/admin/tenants` is listed under the "Administration" nav group.
- Auth shell UX cleanup completed before Sprint 2: `MainLayout` shows Sign In for anonymous users and Sign Out for authenticated users; `/` is public landing; `/dashboard` is protected; normal feature nav links are visible only to `Agent`/`AgencyManager`; admin nav remains Administrator-only.
- `RedirectToLogin` checks auth state: anonymous users go to `/login?returnUrl=...`, while authenticated users without the required route role are redirected to `/` to avoid login redirect loops.
- Login defaults to `/admin/tenants` for `Administrator` and `/dashboard` for tenant users when no explicit return URL is supplied.
- Local launch ports for Visual Studio testing: API `https://localhost:7001`, Web `https://localhost:7002`.
- Do not start the Web/API with `dotnet run` unless the owner explicitly asks for it; manual runtime testing is expected to happen through Visual Studio.
- `JwtAuthStateProvider` must tolerate invalid/stale localStorage tokens and parse numeric JWT claims such as `exp` via raw JSON text; otherwise Blazor can remain stuck on `Authorizing` or show generic login errors after a successful API login.
- `MudDataGrid` `ServerData` delegate in MudBlazor 9.x requires `CancellationToken` as second parameter: `Task<GridData<T>> Load(GridState<T> state, CancellationToken ct)`.
- String interpolation `$"..."` cannot be used directly inside Razor attribute values — use named methods or `@(...)` lambdas that call helper methods instead.
- `MudStack` does not support `JustifyContent` — use `Justify` (e.g. `Justify="Justify.FlexEnd"`).
- `ApiClient.PutAsync<T>` was added in Sprint 2 frontend (#49) — available for all PUT calls going forward.
- `DeleteConfirmDialog` reusable component lives in `Components/DeleteConfirmDialog.razor` — accepts `ContentText` parameter, uses `IMudDialogInstance` cascade.
- `PagedResult<T>` client model is in `Web/Models/Common/PagedResult.cs` (mirrors backend `PagedResult<T>`).
- Buyer pages: `/buyers` (list), `/buyers/create`, `/buyers/{id}` (detail), `/buyers/{id}/edit`.
