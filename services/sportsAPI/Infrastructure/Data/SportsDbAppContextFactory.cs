using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public class SportsDbAppContextFactory : IDesignTimeDbContextFactory<SportsDbAppContext>
{
    public SportsDbAppContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SportsDbAppContext>();

        // Use a default connection string for design-time
        var connectionString = "Server=localhost,1433;Database=SportsDb;User Id=sa;Password=Test123!;TrustServerCertificate=True;";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new SportsDbAppContext(optionsBuilder.Options);
    }
}
