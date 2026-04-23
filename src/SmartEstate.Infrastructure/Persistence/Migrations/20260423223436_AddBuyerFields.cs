using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEstate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BudgetMaxEur",
                table: "Buyers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetMinEur",
                table: "Buyers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Buyers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "PreferredLocations",
                table: "Buyers",
                type: "text[]",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Buyers_TenantId_IsDeleted_FullName",
                table: "Buyers",
                columns: new[] { "TenantId", "IsDeleted", "FullName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buyers_TenantId_IsDeleted_FullName",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "BudgetMaxEur",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "BudgetMinEur",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "PreferredLocations",
                table: "Buyers");
        }
    }
}
