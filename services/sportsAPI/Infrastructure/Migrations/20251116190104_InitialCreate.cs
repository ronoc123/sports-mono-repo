using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamShortName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormedYear = table.Column<int>(type: "int", nullable: true),
                    Sport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Venue_Stadium = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Venue_Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Venue_Capacity = table.Column<int>(type: "int", maxLength: 300, nullable: false),
                    MediaAssets_BadgeUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MediaAssets_LogoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MediaAssets_Fanart1Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MediaAssets_Fanart2Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MediaAssets_Fanart3Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MediaAssets_Fanart4Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MediaAssets_BannerUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MediaAssets_EquipmentUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SocialLinks_Website = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SocialLinks_Facebook = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SocialLinks_Twitter = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SocialLinks_Instagram = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SocialLinks_YoutubeUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TeamColors_Primary = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TeamColors_Secondary = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TeamColors_Tertiary = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Votes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColorPrimary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorSecondary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorTertiary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoteAccounts",
                columns: table => new
                {
                    OrgId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    balance = table.Column<long>(type: "bigint", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteAccounts", x => new { x.OrgId, x.UserId });
                    table.CheckConstraint("ck_vote_accounts_balance_nonneg", "[balance] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "VoteTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrgId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefId = table.Column<long>(type: "bigint", nullable: true),
                    PlayerOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SpendId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Choice = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteTransactions", x => x.Id);
                    table.CheckConstraint("ck_vt_amount_nonzero", "[amount] <> 0");
                    table.CheckConstraint("ck_vt_reason_amount_sign", "([reason] = 'vote_spend' AND [amount] < 0) OR ([reason] <> 'vote_spend' AND [amount] > 0)");
                    table.CheckConstraint("ck_vt_vote_spend_requires_fields", "([reason] <> 'vote_spend') OR ([playerOptionId] IS NOT NULL AND [spendId] IS NOT NULL)");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "PlayerOptions");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropTable(
                name: "VoteAccounts");

            migrationBuilder.DropTable(
                name: "VoteTransactions");
        }
    }
}
