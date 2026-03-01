using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCardsCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardPlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OverallRating = table.Column<int>(type: "int", nullable: false),
                    RarityTier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardPlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RarityTierConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RarityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RatingMin = table.Column<int>(type: "int", nullable: false),
                    RatingMax = table.Column<int>(type: "int", nullable: false),
                    PullWeightBps = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RarityTierConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardOwners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsListed = table.Column<bool>(type: "bit", nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardOwners_CardPlayers_CardPlayerId",
                        column: x => x.CardPlayerId,
                        principalTable: "CardPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardOwners_CardPlayerId",
                table: "CardOwners",
                column: "CardPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_CardOwners_UserId_OrgId",
                table: "CardOwners",
                columns: new[] { "UserId", "OrgId" });

            migrationBuilder.CreateIndex(
                name: "IX_CardPlayers_LeagueId",
                table: "CardPlayers",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_RarityTierConfigs_OrgId_RarityName",
                table: "RarityTierConfigs",
                columns: new[] { "OrgId", "RarityName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardOwners");

            migrationBuilder.DropTable(
                name: "RarityTierConfigs");

            migrationBuilder.DropTable(
                name: "CardPlayers");
        }
    }
}
