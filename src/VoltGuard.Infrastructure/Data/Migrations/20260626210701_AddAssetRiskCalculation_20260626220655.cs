using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltGuard.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetRiskCalculation_20260626220655 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCritical",
                table: "Measurements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "Assets",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_IsCritical",
                table: "Measurements",
                column: "IsCritical");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_RiskLevel",
                table: "Assets",
                column: "RiskLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Measurements_IsCritical",
                table: "Measurements");

            migrationBuilder.DropIndex(
                name: "IX_Assets_RiskLevel",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "IsCritical",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Assets");
        }
    }
}
