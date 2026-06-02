using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace ProviderAssignmentStarter.Data.Migrations
{
    [Microsoft.EntityFrameworkCore.Migrations.Migration("20260529000000_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    ProviderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    County = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.ProviderId);
                });

            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    LicenseId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProviderId = table.Column<int>(type: "INTEGER", nullable: false),
                    LicenseNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LicenseStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.LicenseId);
                    table.ForeignKey(
                        name: "FK_Licenses_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "ProviderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "ProviderId", "ProviderName", "County", "Status", "CreatedDate", "IsDeleted", "DeletedAt" },
                values: new object[,]
                {
                    { 1,  "Fulton Family Care",        "Fulton",   "Active",   new DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc),  false, null },
                    { 2,  "DeKalb Medical Group",      "DeKalb",   "Active",   new DateTime(2023, 3, 22, 0, 0, 0, DateTimeKind.Utc),  false, null },
                    { 3,  "Gwinnett Health Services",  "Gwinnett", "Active",   new DateTime(2023, 6, 10, 0, 0, 0, DateTimeKind.Utc),  false, null },
                    { 4,  "Cobb County Clinics",       "Cobb",     "Inactive", new DateTime(2022, 11, 5, 0, 0, 0, DateTimeKind.Utc),  false, null },
                    { 5,  "Clayton Care Partners",     "Clayton",  "Pending",  new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),   false, null },
                    { 6,  "Cherokee Wellness Center",  "Cherokee", "Active",   new DateTime(2022, 8, 19, 0, 0, 0, DateTimeKind.Utc),  false, null },
                    { 7,  "Henry County Health",       "Henry",    "Active",   new DateTime(2023, 9, 3, 0, 0, 0, DateTimeKind.Utc),   false, null },
                    { 8,  "Archived Provider Inc",     "Fulton",   "Inactive", new DateTime(2021, 5, 14, 0, 0, 0, DateTimeKind.Utc), true,  new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 9,  "Douglas Diagnostic Center", "Douglas",  "Active",   new DateTime(2024, 4, 7, 0, 0, 0, DateTimeKind.Utc),   false, null },
                    { 10, "Forsyth Primary Care",      "Forsyth",  "Pending",  new DateTime(2024, 5, 20, 0, 0, 0, DateTimeKind.Utc),  false, null }
                });

            migrationBuilder.InsertData(
                table: "Licenses",
                columns: new[] { "LicenseId", "ProviderId", "LicenseNumber", "LicenseStatus", "ExpirationDate" },
                values: new object[,]
                {
                    { 1,  1,  "LIC-2023-0001", "Active",    new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { 2,  1,  "LIC-2023-0002", "Suspended", new DateTime(2025, 6, 30, 0, 0, 0, DateTimeKind.Utc) },
                    { 3,  2,  "LIC-2023-0010", "Expired",   new DateTime(2024, 3, 22, 0, 0, 0, DateTimeKind.Utc) },
                    { 4,  2,  "LIC-2023-0011", "Active",    new DateTime(2026, 3, 22, 0, 0, 0, DateTimeKind.Utc) },
                    { 5,  3,  "LIC-2023-0020", "Active",    new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 6,  3,  "LIC-2021-0021", "Expired",   new DateTime(2023, 6, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 7,  4,  "LIC-2022-0030", "Expired",   new DateTime(2023, 11, 5, 0, 0, 0, DateTimeKind.Utc) },
                    { 8,  5,  "LIC-2024-0040", "Active",    new DateTime(2027, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { 9,  6,  "LIC-2022-0050", "Active",    new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 7,  "LIC-2023-0060", "Expired",   new DateTime(2024, 9, 3, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, 8,  "LIC-2021-0070", "Expired",   new DateTime(2022, 5, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 9,  "LIC-2024-0080", "Active",    new DateTime(2027, 4, 7, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, 10, "LIC-2024-0090", "Active",    new DateTime(2027, 5, 20, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_ProviderId",
                table: "Licenses",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_IsDeleted",
                table: "Providers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ProviderName",
                table: "Providers",
                column: "ProviderName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Licenses");
            migrationBuilder.DropTable(name: "Providers");
        }
    }
}
