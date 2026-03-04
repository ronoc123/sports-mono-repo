using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuctionListingLeagueScoped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuctionListings_OrgId_Status",
                table: "AuctionListings");

            migrationBuilder.RenameColumn(
                name: "CardOwnerId",
                table: "AuctionListings",
                newName: "UserCardId");

            migrationBuilder.RenameColumn(
                name: "OrgId",
                table: "AuctionListings",
                newName: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionListings_LeagueId_Status",
                table: "AuctionListings",
                columns: new[] { "LeagueId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuctionListings_LeagueId_Status",
                table: "AuctionListings");

            migrationBuilder.RenameColumn(
                name: "UserCardId",
                table: "AuctionListings",
                newName: "CardOwnerId");

            migrationBuilder.RenameColumn(
                name: "LeagueId",
                table: "AuctionListings",
                newName: "OrgId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionListings_OrgId_Status",
                table: "AuctionListings",
                columns: new[] { "OrgId", "Status" });
        }
    }
}
