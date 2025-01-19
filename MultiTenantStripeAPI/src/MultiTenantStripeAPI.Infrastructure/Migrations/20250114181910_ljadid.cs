using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantStripeAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ljadid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant1-id",
                columns: new[] { "CreatedAt", "LastUpdated" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant2-id",
                columns: new[] { "CreatedAt", "LastUpdated" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant1-id",
                columns: new[] { "CreatedAt", "LastUpdated" },
                values: new object[] { new DateTime(2025, 1, 14, 17, 56, 10, 606, DateTimeKind.Utc).AddTicks(6067), new DateTime(2025, 1, 14, 17, 56, 10, 606, DateTimeKind.Utc).AddTicks(6071) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: "tenant2-id",
                columns: new[] { "CreatedAt", "LastUpdated" },
                values: new object[] { new DateTime(2025, 1, 14, 17, 56, 10, 606, DateTimeKind.Utc).AddTicks(6073), new DateTime(2025, 1, 14, 17, 56, 10, 606, DateTimeKind.Utc).AddTicks(6073) });
        }
    }
}
