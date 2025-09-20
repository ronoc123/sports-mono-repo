dotnet ef migrations add InitialCreate --project "C:\Users\kampe\OneDrive\Desktop\sports-ui\services\sportsAPI\src\SportsAPI\Infrastructure\Infrastructure.csproj" --startup-project "C:\Users\kampe\OneDrive\Desktop\sports-ui\services\sportsAPI\src\SportsAPI\WebAPI\WebAPI.csproj" --context SportsDbAppContext
dotnet ef database update --project "C:\Users\kampe\OneDrive\Desktop\sports-ui\services\sportsAPI\src\SportsAPI\Infrastructure\Infrastructure.csproj" --startup-project "C:\Users\kampe\OneDrive\Desktop\sports-ui\services\sportsAPI\src\SportsAPI\WebAPI\WebAPI.csproj" --context SportsDbAppContext

dotnet ef migrations add InitialCreate --project "C:\Users\kampe\OneDrive\Desktop\sports-ui\services\sportsAPI\src\IdentityService\IdentityService.csproj" --context IdentityDbContext
dotnet ef database update --project "C:\Users\kampe\OneDrive\Desktop\sports-ui\services\sportsAPI\src\IdentityService\IdentityService.csproj" --context IdentityDbContext
