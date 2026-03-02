using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RarityTierAndCardOwnerLeagueScoped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrgId",
                table: "RarityTierConfigs",
                newName: "LeagueId");

            migrationBuilder.RenameIndex(
                name: "IX_RarityTierConfigs_OrgId_RarityName",
                table: "RarityTierConfigs",
                newName: "IX_RarityTierConfigs_LeagueId_RarityName");

            migrationBuilder.RenameColumn(
                name: "OrgId",
                table: "CardOwners",
                newName: "LeagueId");

            migrationBuilder.RenameIndex(
                name: "IX_CardOwners_UserId_OrgId",
                table: "CardOwners",
                newName: "IX_CardOwners_UserId_LeagueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LeagueId",
                table: "RarityTierConfigs",
                newName: "OrgId");

            migrationBuilder.RenameIndex(
                name: "IX_RarityTierConfigs_LeagueId_RarityName",
                table: "RarityTierConfigs",
                newName: "IX_RarityTierConfigs_OrgId_RarityName");

            migrationBuilder.RenameColumn(
                name: "LeagueId",
                table: "CardOwners",
                newName: "OrgId");

            migrationBuilder.RenameIndex(
                name: "IX_CardOwners_UserId_LeagueId",
                table: "CardOwners",
                newName: "IX_CardOwners_UserId_OrgId");
        }
    }
}
