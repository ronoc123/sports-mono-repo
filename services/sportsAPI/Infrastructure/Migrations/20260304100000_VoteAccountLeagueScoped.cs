using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VoteAccountLeagueScoped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrgId",
                table: "VoteAccounts",
                newName: "LeagueId");

            migrationBuilder.RenameColumn(
                name: "OrgId",
                table: "VoteTransactions",
                newName: "LeagueId");

            migrationBuilder.RenameColumn(
                name: "VoteAccountOrgId",
                table: "VoteTransactions",
                newName: "VoteAccountLeagueId");

            migrationBuilder.RenameIndex(
                name: "IX_VoteTransactions_VoteAccountOrgId_VoteAccountUserId",
                table: "VoteTransactions",
                newName: "IX_VoteTransactions_VoteAccountLeagueId_VoteAccountUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_VoteTransactions_VoteAccountLeagueId_VoteAccountUserId",
                table: "VoteTransactions",
                newName: "IX_VoteTransactions_VoteAccountOrgId_VoteAccountUserId");

            migrationBuilder.RenameColumn(
                name: "VoteAccountLeagueId",
                table: "VoteTransactions",
                newName: "VoteAccountOrgId");

            migrationBuilder.RenameColumn(
                name: "LeagueId",
                table: "VoteAccounts",
                newName: "OrgId");

            migrationBuilder.RenameColumn(
                name: "LeagueId",
                table: "VoteTransactions",
                newName: "OrgId");
        }
    }
}
