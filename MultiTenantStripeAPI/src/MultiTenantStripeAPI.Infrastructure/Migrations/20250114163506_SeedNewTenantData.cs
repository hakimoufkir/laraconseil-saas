using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantStripeAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedNewTenantData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrandingLogoUrl",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeactivationReason",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaxUsers",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PlanType",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeColor",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant1-id",
                columns: new[] { "BrandingLogoUrl", "CreatedAt", "DeactivationReason", "LastUpdated", "MaxUsers", "PlanType", "ThemeColor" },
                values: new object[] { null, new DateTime(2025, 1, 14, 16, 35, 6, 63, DateTimeKind.Utc).AddTicks(2769), null, new DateTime(2025, 1, 14, 16, 35, 6, 63, DateTimeKind.Utc).AddTicks(2770), 10, "Grower", null });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant2-id",
                columns: new[] { "BrandingLogoUrl", "CreatedAt", "DeactivationReason", "LastUpdated", "MaxUsers", "PlanType", "ThemeColor" },
                values: new object[] { null, new DateTime(2025, 1, 14, 16, 35, 6, 63, DateTimeKind.Utc).AddTicks(2772), null, new DateTime(2025, 1, 14, 16, 35, 6, 63, DateTimeKind.Utc).AddTicks(2773), 10, "Station", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandingLogoUrl",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeactivationReason",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MaxUsers",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PlanType",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ThemeColor",
                table: "Tenants");
        }
    }
}
