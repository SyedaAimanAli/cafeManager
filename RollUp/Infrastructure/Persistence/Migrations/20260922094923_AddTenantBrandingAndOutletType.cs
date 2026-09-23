using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RollUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantBrandingAndOutletType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeadingFont",
                table: "Tenants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MenuLayout",
                table: "Tenants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverStyle",
                table: "Tenants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptedPaymentMethods",
                table: "Tenants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutletType",
                table: "Outlets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeadingFont",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MenuLayout",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CoverStyle",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "AcceptedPaymentMethods",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "OutletType",
                table: "Outlets");
        }
    }
}
