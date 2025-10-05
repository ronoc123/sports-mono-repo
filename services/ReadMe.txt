dotnet ef migrations add InitialCreate -p .\SportsAPI\Infrastructure\Infrastructure.csproj -s .\SportsAPI\WebAPI\WebAPI.csproj --context SportsDbAppContext
dotnet ef database update -p .\SportsAPI\Infrastructure\Infrastructure.csproj -s .\SportsAPI\WebAPI\WebAPI.csproj --context SportsDbAppContext


dotnet ef migrations add InitialCreate -p .\IdentityService\IdentityService.csproj -s .\IdentityService\IdentityService.csproj --context IdentityDbContext
dotnet ef database update -p .\IdentityService\IdentityService.csproj -s .\IdentityService\IdentityService.csproj --context IdentityDbContext
