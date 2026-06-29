using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltGuard.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTestEvaluationLogic_20260626215109 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WarningMaximumValue",
                table: "Measurements",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WarningMinimumValue",
                table: "Measurements",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_Status",
                table: "Measurements",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Measurements_Status",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "WarningMaximumValue",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "WarningMinimumValue",
                table: "Measurements");
        }
    }
}
