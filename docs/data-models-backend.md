# Data Models — Backend

Generated: 2026-02-22 | Source: EF Core DbContext + Domain entities

---

## Database: SportsDb (SQL Server 2022)

Managed by EF Core 8 code-first migrations. DbContext: `SportsDbAppContext`.

All entities inherit from `Entity` base class providing:
- `Id` (typed value object)
- `CreatedAt` (DateTime?)
- `CreatedBy` (string?)
- `LastModified` (DateTime?)
- `LastModifiedBy` (string?)

Aggregate roots implement `IAggregate` and raise `IDomainEvent` objects dispatched post-save.

---

## Domain Entities

### Organization
- `OrganizationId` (value object)
- `Name` (string)
- `Sport` (string)
- `SocialLinks` (value object)
- `Address` (value object)
- `MediaAssets` (value object — logos, banners)
- `TeamColors` (value object)
- `Venue` (value object)
- **Has**: `Theme` (child entity — `ThemeId`, colors, branding config)
- **Belongs to**: `League`
- **Has many**: `PlayerOption`, `Player`, `RewardItem`, `Notification`

### League
- `LeagueId` (value object)
- `Name` (string)
- **Has many**: `Organization`, `Player`

### Player
- `PlayerId` (value object)
- Profile fields (name, position, etc.)
- **Belongs to**: `League`, `Organization`

### PlayerOption
- `PlayerOptionId` (value object)
- Voting option details (player reference, metadata)
- Vote count tracking
- **Belongs to**: `Organization`

### VoteAccount
- `VoteAccountId` (value object)
- `UserId` (value object — links to IdentityService user)
- `OrganizationId` (FK)
- Balance / token fields
- **Has many**: `VoteTransaction`

### VoteTransaction
- `VoteAccountId` (FK)
- `SpendToken` (shared kernel value object)
- Transaction metadata (amount, type, timestamp)

### RewardItem
- `RewardItemId` (value object)
- `ProductType` (enum: ProductType)
- `PromoCode` (string — added in migration `rewarditempromocode`)
- Price / fulfillment metadata
- `FulfillmentStatus` (enum)
- **Belongs to**: `Organization`

### Notification
- `NotificationId` (value object)
- `UserId` / recipient reference
- Message, read status, timestamp
- **Belongs to**: `Organization`

### Product *(Domain model)*
- `ProductType` (enum)
- Price, metadata

### Purchase *(Domain model)*
- `PurchaseStatus` (enum)
- `PaymentProvider` (enum: Stripe, etc.)
- `ExternalPaymentId`, `ExternalSessionId`, `StripePaymentIntentId`, `StripeSessionId` (value objects)
- `PurchaseItem` (value object — line items)

---

## Value Objects

| Name | Description |
|------|-------------|
| `Address` | Street, city, country |
| `SocialLinks` | Twitter, Instagram, etc. |
| `MediaAssets` | Logo URL, banner URL |
| `TeamColors` | Primary / secondary hex colors |
| `Venue` | Stadium name, location |
| `Money` | Amount + currency |
| `RedeptionToken` | Redemption code token |
| `SpendToken` | Vote spend token |
| `PurchaseItem` | Line-item in a purchase |
| `ExternalPaymentId` | Payment provider reference |
| `StripeSessionId` / `StripePaymentIntentId` | Stripe-specific payment identifiers |

---

## Enums

| Enum | Values |
|------|--------|
| `FulfillmentStatus` | Pending, Fulfilled, etc. |
| `PaymentProvider` | Stripe, etc. |
| `ProductType` | Physical, Digital, etc. |
| `PurchaseStatus` | Pending, Completed, Failed, etc. |
| `Status` | Active, Inactive, etc. |

---

## EF Core Migrations (sportsAPI)

| Migration | Date | Description |
|-----------|------|-------------|
| `VoteTransactionEntity2` | 2025-12-12 | VoteTransaction entity added |
| `Notificationv2` | 2025-12-28 | Notification entity v2 |
| `rewarditempromocode` | 2026-01-01 | PromoCode field added to RewardItem |

---

## IdentityService Database

DbContext: `IdentityDbContext` (ASP.NET Core Identity)

### ApplicationUser
Extends `IdentityUser`:
- Standard Identity fields (Id, Email, PasswordHash, etc.)
- Custom fields as defined in `ApplicationUser.cs`

### Standard Identity Tables
- `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`
- `AspNetUserClaims`, `AspNetRoleClaims`
- `AspNetUserTokens`, `AspNetUserLogins`

### Migration Commands
```bash
# SportsAPI
dotnet ef migrations add <Name> -p .\sportsAPI\Infrastructure\Infrastructure.csproj -s .\sportsAPI\WebAPI\WebAPI.csproj --context SportsDbAppContext
dotnet ef database update -p .\sportsAPI\Infrastructure\Infrastructure.csproj -s .\sportsAPI\WebAPI\WebAPI.csproj --context SportsDbAppContext

# IdentityService
dotnet ef migrations add <Name> -p .\IdentityService\IdentityService.csproj -s .\IdentityService\IdentityService.csproj --context IdentityDbContext
dotnet ef database update -p .\IdentityService\IdentityService.csproj -s .\IdentityService\IdentityService.csproj --context IdentityDbContext
```
