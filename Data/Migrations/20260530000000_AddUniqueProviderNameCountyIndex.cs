using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProviderAssignmentStarter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueProviderNameCountyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Providers_ProviderName",
                table: "Providers");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ProviderName_County",
                table: "Providers",
                columns: new[] { "ProviderName", "County" },
                unique: true,
                filter: "\"IsDeleted\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Providers_ProviderName_County",
                table: "Providers");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ProviderName",
                table: "Providers",
                column: "ProviderName");
        }
    }
}
