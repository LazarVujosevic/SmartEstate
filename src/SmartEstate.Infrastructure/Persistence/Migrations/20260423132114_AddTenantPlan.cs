using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEstate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Plan",
                table: "Tenants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Tenants");
        }
    }
}
