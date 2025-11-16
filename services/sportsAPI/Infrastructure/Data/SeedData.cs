using Microsoft.EntityFrameworkCore;
using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.Users;
using Domain.Leagues;
using Domain.ValueObjects.ConcreteTypes;
using Domain.ValueObjects;

namespace Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(SportsDbAppContext context)
        {
            try
            {
                // Check if data already exists
                if (await context.Organizations.AnyAsync())
                {
                    Console.WriteLine("🔄 Database already seeded, skipping...");
                    return; // Database has been seeded
                }

                Console.WriteLine("🌱 Starting database seeding...");

                // Seed Leagues first
                await SeedLeagues(context);
                Console.WriteLine("✅ Leagues seeded");

                Console.WriteLine("✅ NFL Organizations seeded");

                Console.WriteLine("🎉 Basic seed data created successfully!");
                Console.WriteLine("📊 Seeded: 1 League (NFL), 1 Organization (Kansas City Chiefs)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Seeding failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Don't throw - let the app continue running
            }
        }

        private static async Task SeedLeagues(SportsDbAppContext context)
        {
            var leagues = new List<League>
            {
                League.Create(
                    "NFL"
                ),
                League.Create(
                    "NBA"
                ),
                League.Create(
                    "MLB"
                )
            };

            context.Leagues.AddRange(leagues);
            await context.SaveChangesAsync();
        }



    }
}
