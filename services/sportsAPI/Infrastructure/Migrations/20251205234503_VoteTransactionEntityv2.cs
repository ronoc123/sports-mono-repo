using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VoteTransactionEntityv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_vote_accounts_balance_nonneg",
                table: "VoteAccounts");

            migrationBuilder.RenameColumn(
                name: "version",
                table: "VoteAccounts",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "balance",
                table: "VoteAccounts",
                newName: "Balance");

            migrationBuilder.AlterColumn<string>(
                name: "RefId",
                table: "VoteTransactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "VoteAccounts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "VoteAccounts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Version",
                table: "VoteAccounts",
                newName: "version");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "VoteAccounts",
                newName: "balance");

            migrationBuilder.AlterColumn<string>(
                name: "RefId",
                table: "VoteTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "VoteAccounts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "VoteAccounts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vote_accounts_balance_nonneg",
                table: "VoteAccounts",
                sql: "[balance] >= 0");
        }
    }
}
