using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantStripeAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTenantSchem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenantName",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubscriptionStatus",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlanType",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DatabaseConnectionString",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant1-id",
                columns: new[] { "CreatedAt", "LastUpdated", "MaxUsers" },
                values: new object[] { new DateTime(2025, 1, 14, 17, 56, 10, 606, DateTimeKind.Utc).AddTicks(6067), new DateTime(2025, 1, 14, 17, 56, 10, 606, DateTimeKind.Utc).AddTicks(6071), 0 });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant2-id",
                columns: new[] { "CreatedAt", "LastUpdated", "MaxUsers" },
                values: new object[] { new DateTime(2025, 1, 14, 17, 56, 10, 606, DateTimeKind.Utc).AddTicks(6073), new DateTime(2025, 1, 14, 17, 56, 10, 606, DateTimeKind.Utc).AddTicks(6073), 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenantName",
                table: "Tenants",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SubscriptionStatus",
                table: "Tenants",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PlanType",
                table: "Tenants",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tenants",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DatabaseConnectionString",
                table: "Tenants",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant1-id",
                columns: new[] { "CreatedAt", "LastUpdated", "MaxUsers" },
                values: new object[] { new DateTime(2025, 1, 14, 16, 35, 6, 63, DateTimeKind.Utc).AddTicks(2769), new DateTime(2025, 1, 14, 16, 35, 6, 63, DateTimeKind.Utc).AddTicks(2770), 10 });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant2-id",
                columns: new[] { "CreatedAt", "LastUpdated", "MaxUsers" },
                values: new object[] { new DateTime(2025, 1, 14, 16, 35, 6, 63, DateTimeKind.Utc).AddTicks(2772), new DateTime(2025, 1, 14, 16, 35, 6, 63, DateTimeKind.Utc).AddTicks(2773), 10 });
        }
    }
}
