using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltGuard.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTestResultsFeature_20260626212430 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssetId",
                table: "TestResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "TestResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EngineerName",
                table: "TestResults",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstrumentModel",
                table: "TestResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstrumentSerialNumber",
                table: "TestResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TestResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextTestDueAtUtc",
                table: "TestResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TestResults",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OverallStatus",
                table: "TestResults",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "TestResults",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "TestResults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TestDateUtc",
                table: "TestResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TestReference",
                table: "TestResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestType",
                table: "TestResults",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "TestResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Measurements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumAllowedValue",
                table: "Measurements",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeasurementType",
                table: "Measurements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumAllowedValue",
                table: "Measurements",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Measurements",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phase",
                table: "Measurements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Measurements",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TestResultId",
                table: "Measurements",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Measurements",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Measurements",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_AssetId",
                table: "TestResults",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_OverallStatus",
                table: "TestResults",
                column: "OverallStatus");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_RiskLevel",
                table: "TestResults",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_TestDateUtc",
                table: "TestResults",
                column: "TestDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_TestReference",
                table: "TestResults",
                column: "TestReference");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_MeasurementType",
                table: "Measurements",
                column: "MeasurementType");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_TestResultId",
                table: "Measurements",
                column: "TestResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_Measurements_TestResults_TestResultId",
                table: "Measurements",
                column: "TestResultId",
                principalTable: "TestResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_Assets_AssetId",
                table: "TestResults",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_TestResults_TestResultId",
                table: "Measurements");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_Assets_AssetId",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_TestResults_AssetId",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_TestResults_OverallStatus",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_TestResults_RiskLevel",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_TestResults_TestDateUtc",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_TestResults_TestReference",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_MeasurementType",
                table: "Measurements");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_TestResultId",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "EngineerName",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "InstrumentModel",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "InstrumentSerialNumber",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "NextTestDueAtUtc",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "OverallStatus",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "TestDateUtc",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "TestReference",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "TestType",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "MaximumAllowedValue",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "MeasurementType",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "MinimumAllowedValue",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Phase",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "TestResultId",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Measurements");
        }
    }
}
