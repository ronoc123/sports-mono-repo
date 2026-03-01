using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddH2HTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "H2HMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FanUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WagerAmount = table.Column<long>(type: "bigint", nullable: false),
                    FanTeamOverall = table.Column<int>(type: "int", nullable: false),
                    BotTeamOverall = table.Column<int>(type: "int", nullable: false),
                    BotSquadSnapshot = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false, defaultValue: "[]"),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_H2HMatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "H2HSquadCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardOwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlotIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_H2HSquadCards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_H2HMatches_FanUserId_OrgId",
                table: "H2HMatches",
                columns: new[] { "FanUserId", "OrgId" });

            migrationBuilder.CreateIndex(
                name: "IX_H2HMatches_Status",
                table: "H2HMatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_H2HSquadCards_MatchId",
                table: "H2HSquadCards",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "H2HMatches");

            migrationBuilder.DropTable(
                name: "H2HSquadCards");
        }
    }
}
