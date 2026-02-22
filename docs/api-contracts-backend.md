# API Contracts — Backend (sportsAPI + IdentityService)

Generated: 2026-02-22 | Scan Level: Deep

---

## sportsAPI (port 5000 / 8080 Docker)

Base URL: `http://localhost:5000/api`

All responses are wrapped in `ServiceResponse<T>` unless noted.

---

### League Controller — `/api/League`

| Method | Path | Description | Request Body |
|--------|------|-------------|--------------|
| POST | `/api/League/add` | Create a new league | `CreateLeagueCommand` |
| GET | `/api/League/all` | Get all leagues | — |
| PUT | `/api/League/update` | Update a league | `UpdateLeagueCommand` |
| DELETE | `/api/League/delete` | Delete a league | `DeleteLeagueCommand` |

---

### Organization Controller — `/api/Org`

| Method | Path | Description | Request Body / Query Params |
|--------|------|-------------|--------------|
| GET | `/api/Org/GetAllOrganization` | Paginated org list with filters | `?pageNumber=1&pageSize=50&searchTerm&leagueId&sport&sortBy=Name&sortDescending=false` |
| PUT | `/api/Org/updateOrganization` | Update an organization | `UpdateOrganizationCommand` |
| DELETE | `/api/Org/deleteOrganization/{organizationId}` | Delete an organization | Route: `organizationId` (GUID) |
| GET | `/api/Org/theme` | Get theme by name | `?name=string` |
| GET | `/api/Org/organizationDetails` | Get organization details | `?organizationId=GUID` |
| POST | `/api/Org/addOrganization` | Create a new organization | `CreateOrganizationCommand` |

---

### Player Controller — `/api/Player`

| Method | Path | Description | Request Body / Query |
|--------|------|-------------|----------------------|
| GET | `/api/Player/all` | Get all players for a league | `?leagueId=GUID` |
| PUT | `/api/Player/update` | Update a player | `UpdatePlayerCommand` |
| DELETE | `/api/Player/delete/{playerId}` | Delete a player | Route: `playerId` |

---

### PlayerOption Controller — `/api/PlayerOption`

| Method | Path | Description | Request Body / Query |
|--------|------|-------------|----------------------|
| PUT | `/api/PlayerOption/update` | Update a player option | `UpdatePlayerOptionCommand` |
| GET | `/api/PlayerOption/GetPlayerOptionsByOrganization` | Get player options by org | `?organizationId=GUID` |
| POST | `/api/PlayerOption/create` | Create a player option | `CreatePlayerOptionCommand` |
| POST | `/api/PlayerOption/vote` | Cast a vote on a player option | `VoteCommand` |

---

### VoteAccount Controller — `/api/VoteAccount`

| Method | Path | Description | Request Body |
|--------|------|-------------|--------------|
| GET | `/api/VoteAccount/get-vote-account/{userId}` | Get vote account for a user | — |
| GET | `/api/VoteAccount/get-vote-account/{userId}/organization/{organizationId}` | Get vote account by user + org | — |
| POST | `/api/VoteAccount/redeem-vote/{userId}/reward/{rewardItemId}` | Redeem a reward item | — |
| POST | `/api/VoteAccount/reward-for-user` | Grant reward tokens to a user | Request body TBD |

---

### Notification Controller — `/api/Notification`

| Method | Path | Description | Request Body |
|--------|------|-------------|--------------|
| GET | `/api/Notification/get-all` | Get all notifications | — |
| POST | `/api/Notification/{notificationId}/read` | Mark a notification as read | Route: `notificationId` |

---

## IdentityService (port 5001 / 8080 Docker)

Base URL: `http://localhost:5001/api`

### Auth Controller — `/api/Auth`

| Method | Path | Description | Request Body |
|--------|------|-------------|--------------|
| POST | `/api/Auth/register` | Register a new user | `RegisterRequest` |
| POST | `/api/Auth/login` | Login with email/password | `LoginRequest` |
| POST | `/api/Auth/google` | Login / register via Google token | `GoogleAuthRequest` |
| POST | `/api/Auth/refresh` | Refresh access token | `RefreshRequest` |
| POST | `/api/Auth/logout` | Logout and revoke token | — |
| GET | `/api/Auth/get` | Get current user profile | Bearer token required |

---

## Response Envelope

Most sportsAPI endpoints return:

```json
{
  "data": <T>,
  "isSuccess": true,
  "error": null
}
```

IdentityService returns raw DTOs defined in `AuthenticationDTOs.cs`.

---

## Authentication

- All sportsAPI routes consume a **JWT Bearer token** issued by IdentityService
- IdentityService issues tokens via `/api/Auth/login` or `/api/Auth/google`
- Token carried in `Authorization: Bearer <token>` header (injected by `apiBaseUrlInterceptor` in frontend)
- JWT config: Issuer = `SportsAPI.IdentityService`, Audience = `SportsAPI.Client`
