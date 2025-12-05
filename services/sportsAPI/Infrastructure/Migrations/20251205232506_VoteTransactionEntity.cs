using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VoteTransactionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_vt_amount_nonzero",
                table: "VoteTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vt_reason_amount_sign",
                table: "VoteTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vt_vote_spend_requires_fields",
                table: "VoteTransactions");

            migrationBuilder.DropColumn(
                name: "Choice",
                table: "VoteTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "SpendId",
                table: "VoteTransactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefId",
                table: "VoteTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "VoteTransactions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "VoteAccountOrgId",
                table: "VoteTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoteAccountUserId",
                table: "VoteTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoteTransactions_VoteAccountOrgId_VoteAccountUserId",
                table: "VoteTransactions",
                columns: new[] { "VoteAccountOrgId", "VoteAccountUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_VoteTransactions_VoteAccounts_VoteAccountOrgId_VoteAccountUserId",
                table: "VoteTransactions",
                columns: new[] { "VoteAccountOrgId", "VoteAccountUserId" },
                principalTable: "VoteAccounts",
                principalColumns: new[] { "OrgId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoteTransactions_VoteAccounts_VoteAccountOrgId_VoteAccountUserId",
                table: "VoteTransactions");

            migrationBuilder.DropIndex(
                name: "IX_VoteTransactions_VoteAccountOrgId_VoteAccountUserId",
                table: "VoteTransactions");

            migrationBuilder.DropColumn(
                name: "VoteAccountOrgId",
                table: "VoteTransactions");

            migrationBuilder.DropColumn(
                name: "VoteAccountUserId",
                table: "VoteTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "SpendId",
                table: "VoteTransactions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "RefId",
                table: "VoteTransactions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "VoteTransactions",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<int>(
                name: "Choice",
                table: "VoteTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_vt_amount_nonzero",
                table: "VoteTransactions",
                sql: "[amount] <> 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vt_reason_amount_sign",
                table: "VoteTransactions",
                sql: "([reason] = 'vote_spend' AND [amount] < 0) OR ([reason] <> 'vote_spend' AND [amount] > 0)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vt_vote_spend_requires_fields",
                table: "VoteTransactions",
                sql: "([reason] <> 'vote_spend') OR ([playerOptionId] IS NOT NULL AND [spendId] IS NOT NULL)");
        }
    }
}
