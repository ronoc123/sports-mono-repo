# Architecture: BFF Authorization Layer & RBAC

## Status: Draft
## Date: 2026-03-07

---

## 1. System Topology

### Current (Before BFF)

```
Browser (Angular)
  ├── POST :5001/api/auth/**        → IdentityService (login/register/refresh)
  └── GET/POST :5000/api/**        → sports API (all other calls)
```

### Target (After BFF)

```
Browser (Angular)
  ├── POST :5001/api/auth/**        → IdentityService  (unchanged)
  └── GET/POST :5002/api/**        → sports-bff
                                        └── forwards to :5000/api/**  → sports API
```

- The sports API URL (`:5000`) is never exposed to the browser.
- The BFF is the only public-facing entry point for data operations.
- IdentityService remains a direct call from the browser (login/register/OAuth do not go through the BFF).

---

## 2. sports-bff Service

### 2.1 Technology

| Concern | Decision |
|---|---|
| Runtime | .NET 8 Web API |
| Proxy engine | **YARP** (Microsoft.ReverseProxy) |
| Auth | JWT Bearer, RS256, same RSA public key as sports API |
| Port | `:5002` (dev) |
| Project path | `services/sports-bff/` |

**Why YARP?** YARP is Microsoft's production-grade reverse proxy for .NET. It handles header forwarding, load balancing, and middleware integration natively, eliminating manual `HttpClient` plumbing.

### 2.2 Project Structure

```
services/sports-bff/
  sports-bff.csproj
  Program.cs
  appsettings.json
  appsettings.Development.json
  Authorization/
    RoleAuthorizationPolicy.cs        ← route-to-policy mappings
    BffAuthorizationMiddleware.cs     ← validates JWT, checks coarse policies
  Keys/
    public.pem                        ← RSA public key (same as sports API)
```

### 2.3 Dependencies (NuGet)

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.x" />
<PackageReference Include="Yarp.ReverseProxy" Version="2.x" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.x" />
```

### 2.4 appsettings.json

```json
{
  "SportsApi": {
    "BaseUrl": "http://localhost:5000"
  },
  "Jwt": {
    "Issuer": "SportsAPI.IdentityService",
    "Audience": "SportsAPI.Client"
  },
  "ReverseProxy": {
    "Routes": {
      "sports-api-route": {
        "ClusterId": "sports-api-cluster",
        "Match": { "Path": "/api/{**catch-all}" }
      }
    },
    "Clusters": {
      "sports-api-cluster": {
        "Destinations": {
          "primary": {
            "Address": "http://localhost:5000"
          }
        }
      }
    }
  }
}
```

### 2.5 Program.cs Flow

```csharp
// 1. Load RSA public key
var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText("Keys/public.pem"));

// 2. Add JWT validation (RS256)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true, ValidateAudience = true,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new RsaSecurityKey(rsa) { KeyId = "sportify-rsa-1" },
            ClockSkew = TimeSpan.Zero
        };
    });

// 3. Add authorization policies
builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("GMOrAbove", p => p.RequireRole("Admin", "GM"));
    options.AddPolicy("AnyUser",   p => p.RequireAuthenticatedUser());
});

// 4. Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 5. CORS (same as sports API dev config)
builder.Services.AddCors(...);

// Pipeline
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BffAuthorizationMiddleware>();  // coarse-grained route checks
app.MapReverseProxy();
```

### 2.6 BffAuthorizationMiddleware

A middleware (rather than per-route attributes, since YARP handles routing, not controllers) maps request patterns to required policies:

```csharp
// Route-to-policy map (loaded from config, not hardcoded)
private static readonly Dictionary<(string Method, string PathPrefix), string> _routePolicies = new()
{
    { ("ANY", "/api/admin/"),               "AdminOnly" },
    { ("POST", "/api/poll"),                "GMOrAbove" },
    { ("PUT", "/api/poll/"),                "GMOrAbove" },
    { ("POST", "/api/trivia/series"),       "GMOrAbove" },
    { ("POST", "/api/trivia/questions"),    "GMOrAbove" },
    { ("PUT", "/api/trivia/questions/"),    "GMOrAbove" },
    { ("POST", "/api/org/addOrganization"), "GMOrAbove" },
    { ("PUT", "/api/org/"),                 "GMOrAbove" },
    { ("POST", "/api/league/add"),          "GMOrAbove" },
    // Read-only endpoints and fan actions use "AnyUser" (just require valid JWT)
};
```

For any path not in the map, the default is `AnyUser` (valid JWT required). This means the BFF is an opt-in elevation model — by default every endpoint requires login, specific paths require a higher role.

---

## 3. Role & Permission Model

### 3.1 Roles

Roles are stored in ASP.NET Identity `AspNetRoles` table (IdentityService DB).

| Role Name | Description |
|---|---|
| `Admin` | Full system access |
| `GM` | Operations + fan features; no administration |
| `User` | Fan features only |

### 3.2 Permission Keys

Permissions are flat string keys in format `<category>:<action>`.

**Feature permissions** (control nav tab visibility):

| Key | Description |
|---|---|
| `feature:dashboard` | Dashboard tab |
| `feature:collection` | Card collection tab |
| `feature:h2h` | Head-to-head tab |
| `feature:franchise` | Franchise tab |
| `feature:store` | Store & Marketplace tab |
| `feature:operations` | Operations tab (send votes, create options, trivia/poll mgmt) |
| `feature:administration` | Administration tab (economy admin, audit log) |

**Action permissions** (control inline UI elements):

| Key | Description |
|---|---|
| `action:vote` | Submit a player option vote |
| `action:answer-trivia` | Submit a trivia answer |
| `action:submit-poll-vote` | Vote on a poll |
| `action:purchase-pack` | Purchase a card pack |
| `action:create-listing` | Create a marketplace listing |
| `action:create-player-option` | Create/manage player options |
| `action:manage-polls` | Create/archive polls |
| `action:manage-trivia` | Create/publish/archive trivia |
| `action:send-votes` | Send vote rewards to users |
| `action:manage-players` | Update/delete player records |
| `action:economy-admin` | Manage rarity tiers and pack costs |
| `action:audit-log` | View transaction audit log |

### 3.3 Default Role-Permission Mapping

Computed at login time by IdentityService; stored in the JWT as a `permissions` claim and also returned in `UserInfo.Permissions`.

```csharp
// IdentityService/Services/PermissionService.cs
public static class RolePermissions
{
    private static readonly string[] UserPermissions = new[]
    {
        "feature:dashboard", "feature:collection", "feature:h2h",
        "feature:franchise", "feature:store",
        "action:vote", "action:answer-trivia", "action:submit-poll-vote",
        "action:purchase-pack", "action:create-listing"
    };

    private static readonly string[] GmPermissions = UserPermissions.Concat(new[]
    {
        "feature:operations",
        "action:create-player-option", "action:manage-polls",
        "action:manage-trivia", "action:send-votes", "action:manage-players"
    }).ToArray();

    private static readonly string[] AdminPermissions = GmPermissions.Concat(new[]
    {
        "feature:administration",
        "action:economy-admin", "action:audit-log"
    }).ToArray();

    public static IEnumerable<string> ForRole(string role) => role switch
    {
        "Admin" => AdminPermissions,
        "GM"    => GmPermissions,
        "User"  => UserPermissions,
        _       => Array.Empty<string>()
    };
}
```

### 3.4 JWT Claims

The access token includes:
- `role` claim(s): `"Admin"`, `"GM"`, or `"User"`
- `permissions` claim: JSON array of permission keys (added to token by `TokenService.GenerateAccessTokenAsync`)

---

## 4. IdentityService Changes

### 4.1 UserInfo DTO

Extend `UserInfo` to include permissions:

```csharp
// IdentityService/DTOs/UserInfo.cs
public class UserInfo
{
    // ... existing fields ...
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();  // ← NEW
}
```

### 4.2 TokenService.GenerateAccessTokenAsync

Add permissions claim to JWT:

```csharp
var roles = await _userManager.GetRolesAsync(user);
var permissions = roles.SelectMany(RolePermissions.ForRole).Distinct();

var claims = new List<Claim>
{
    new(JwtRegisteredClaimNames.Sub, user.Id),
    new(JwtRegisteredClaimNames.Email, user.Email!),
    // ... existing claims ...
};

foreach (var role in roles)
    claims.Add(new Claim(ClaimTypes.Role, role));

foreach (var perm in permissions)
    claims.Add(new Claim("permissions", perm));
```

### 4.3 AuthController — UserInfo population

In `Register`, `Login`, `GoogleCallback`, and `RefreshToken` endpoints, populate `Permissions` on the returned `UserInfo`:

```csharp
var roles = await _userManager.GetRolesAsync(user);
var permissions = roles.SelectMany(RolePermissions.ForRole).Distinct().ToList();

return Ok(new AuthenticationResponse {
    User = new UserInfo {
        // ...
        Roles = roles.ToList(),
        Permissions = permissions   // ← NEW
    }
});
```

---

## 5. Angular Changes

### 5.1 Auth Model

```typescript
// libs/auth/auth-data-access/src/auth.model.ts
export interface SessionUser {
  // ... existing fields ...
  roles: string[];
  permissions: string[];   // ← NEW
}
```

### 5.2 AuthStore — New Computed Signals

```typescript
// In signalStore withComputed()
hasRole: (role: string) => computed(() =>
  state.user()?.roles?.map(r => r.toLowerCase()).includes(role.toLowerCase()) ?? false
),
hasPermission: (perm: string) => computed(() =>
  state.user()?.permissions?.includes(perm) ?? false
),
```

Because signal store computed signals are not parameterized, expose these as methods on the AuthFacade instead:

### 5.3 AuthFacade — New Methods

```typescript
// libs/auth/auth-data-access/src/services/auth.facade.ts
hasRole(role: string): boolean {
  return this.store.user()?.roles
    ?.map(r => r.toLowerCase())
    .includes(role.toLowerCase()) ?? false;
}

hasPermission(permission: string): boolean {
  return this.store.user()?.permissions?.includes(permission) ?? false;
}

canAccess(feature: `feature:${string}`): boolean {
  return this.hasPermission(feature);
}
```

### 5.4 HasPermission Structural Directive

New lib: `libs/core/rbac/` (or add to existing `libs/ui/`)

```typescript
// libs/core/rbac/src/lib/has-permission.directive.ts
@Directive({ selector: '[hasPermission]', standalone: true })
export class HasPermissionDirective implements OnInit {
  private auth = inject(AuthFacade);
  private vcr = inject(ViewContainerRef);
  private tpl = inject(TemplateRef<any>);

  @Input() set hasPermission(permission: string) {
    this.vcr.clear();
    if (this.auth.hasPermission(permission)) {
      this.vcr.createEmbeddedView(this.tpl);
    }
  }
}

// libs/core/rbac/src/lib/has-role.directive.ts
@Directive({ selector: '[hasRole]', standalone: true })
export class HasRoleDirective {
  private auth = inject(AuthFacade);
  private vcr = inject(ViewContainerRef);
  private tpl = inject(TemplateRef<any>);

  @Input() set hasRole(role: string) {
    this.vcr.clear();
    if (this.auth.hasRole(role)) {
      this.vcr.createEmbeddedView(this.tpl);
    }
  }
}
```

Export from `libs/core/rbac/src/index.ts` and add to `tsconfig.base.json` as `@sports-ui/rbac`.

### 5.5 Role Guard

```typescript
// libs/auth/auth-data-access/src/services/role.guard.ts
export const roleGuard = (requiredPermission: string): CanActivateFn =>
  () => {
    const auth = inject(AuthFacade);
    const router = inject(Router);
    if (auth.hasPermission(requiredPermission)) return true;
    return router.createUrlTree(['/unauthorized']);
  };
```

Usage in routes:

```typescript
{
  path: 'admin',
  canActivate: [authGuard, roleGuard('feature:administration')],
  loadChildren: () => import('@sports-ui/feature-admin').then(m => m.adminRoutes),
}
```

### 5.6 ShellComponent — Dynamic Nav Filtering

```typescript
// shell.component.ts
readonly auth = inject(AuthFacade);

get filteredNavItems(): NavItem[] {
  return this.allNavItems.filter(item =>
    !item.requiredPermission || this.auth.hasPermission(item.requiredPermission)
  );
}

private allNavItems: (NavItem & { requiredPermission?: string })[] = [
  { name: 'Dashboard', route: 'dashboard' },
  { name: 'Collection', route: 'collection' },
  { name: 'H2H', route: 'h2h' },
  { name: 'Franchise', route: 'franchise', children: [...] },
  { name: 'Store & Marketplace', route: 'store', children: [...] },
  {
    name: 'Operations', requiredPermission: 'feature:operations',
    children: [...]
  },
  {
    name: 'Administration', requiredPermission: 'feature:administration',
    children: [...]
  },
];
```

---

## 6. Environment Configuration Changes

```typescript
// libs/core/api-types/src/lib/environments/environment.ts
export const environment = {
  // ...
  sportsApi: ':5002/api/',       // ← points to BFF, not sports API directly
  identityApi: ':5001/api/',     // ← unchanged (direct to IdentityService)
};
```

All existing Angular services that use `environment.sportsApi` automatically route through the BFF without code changes.

---

## 7. New Service: sports-bff Port Assignment

| Service | Port |
|---|---|
| IdentityService | `:5001` |
| sports API | `:5000` (internal only after BFF is live) |
| sports-bff | `:5002` (new public entry point) |

---

## 8. Key Architectural Decisions

### ADR-1: YARP over manual HttpClient proxy
YARP handles connection pooling, header forwarding, streaming responses, and WebSocket proxying (needed for SignalR hubs) out of the box. Manual `HttpClient` forwarding would require reimplementing all of this.

### ADR-2: Permissions as flat string array (not bitmask or nested object)
Flat string arrays are trivially serializable to JSON and JWT claims, easy to extend with new keys, and simple to check with `array.includes(key)`. Bitmasks don't scale beyond 64 permissions; nested objects complicate partial checks.

### ADR-3: Permissions computed at login time, not fetched per-request
Computing permissions from roles at login and embedding them in the JWT means zero additional DB lookups for permission checks. The tradeoff is that permission changes don't take effect until the next login/token refresh. This is acceptable — role changes are an admin action, and the refresh token flow handles it.

### ADR-4: sports API retains [Authorize] as defense-in-depth
The BFF is not trusted to be the only authorization boundary. The sports API still validates JWTs and enforces its own `[Authorize]` attributes. This provides layered security — a misconfigured BFF cannot expose unprotected sports API endpoints.

### ADR-5: NavItem filtering via permissions (not role names)
Nav items check `feature:xxx` permissions rather than role names directly. This means if a future role needs access to Operations (e.g., a `Moderator` role), you add `feature:operations` to that role's permission set — no changes needed in the component.

---

## 9. Implementation Order

1. **IdentityService** — Add `RolePermissions` service, extend `UserInfo` DTO, populate `Permissions` in all auth responses, add `permissions` claim to JWT.
2. **Angular auth model** — Add `permissions` to `SessionUser`, extend `AuthFacade` with helpers.
3. **Angular RBAC lib** — Create `libs/core/rbac` with directives and `roleGuard`.
4. **ShellComponent** — Filter nav items dynamically.
5. **Route guards** — Add `roleGuard` to admin/operations routes.
6. **sports-bff** — Create new .NET 8 service with YARP + JWT validation + `BffAuthorizationMiddleware`.
7. **Environment** — Switch `sportsApi` to point at BFF port.
8. **SignalR** — Configure YARP to proxy WebSocket connections for `/hubs/**`.
