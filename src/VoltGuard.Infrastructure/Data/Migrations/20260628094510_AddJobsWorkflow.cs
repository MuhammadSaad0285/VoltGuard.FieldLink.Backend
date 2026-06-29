using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltGuard.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobsWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [Jobs]");

            migrationBuilder.AddColumn<Guid>(
                name: "AssetId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "Jobs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Jobs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionNotes",
                table: "Jobs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Jobs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Jobs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueAtUtc",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobType",
                table: "Jobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Inspection");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Jobs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Jobs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Medium");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAtUtc",
                table: "Jobs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Scheduled");

            migrationBuilder.AddColumn<Guid>(
                name: "TestResultId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Jobs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_AssetId",
                table: "Jobs",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_DueAtUtc",
                table: "Jobs",
                column: "DueAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobType",
                table: "Jobs",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Priority",
                table: "Jobs",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ScheduledAtUtc",
                table: "Jobs",
                column: "ScheduledAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TestResultId",
                table: "Jobs",
                column: "TestResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Assets_AssetId",
                table: "Jobs",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_TestResults_TestResultId",
                table: "Jobs",
                column: "TestResultId",
                principalTable: "TestResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Assets_AssetId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_TestResults_TestResultId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_AssetId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_DueAtUtc",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobType",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Priority",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ScheduledAtUtc",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Status",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_TestResultId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CompletionNotes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "DueAtUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ScheduledAtUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "TestResultId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Jobs");
        }
    }
}
