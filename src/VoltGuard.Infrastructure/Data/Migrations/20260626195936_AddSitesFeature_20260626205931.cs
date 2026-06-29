using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltGuard.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSitesFeature_20260626205931 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Sites",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Sites",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Sites",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Sites",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Sites",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Sites",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Sites",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Sites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Sites",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sites",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Sites",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Sites",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Postcode",
                table: "Sites",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteCode",
                table: "Sites",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteType",
                table: "Sites",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Sites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_CustomerId",
                table: "Sites",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_CustomerId_Name",
                table: "Sites",
                columns: new[] { "CustomerId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Sites_SiteCode",
                table: "Sites",
                column: "SiteCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Customers_CustomerId",
                table: "Sites",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Customers_CustomerId",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Sites_CustomerId",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Sites_CustomerId_Name",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Sites_SiteCode",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Postcode",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "SiteCode",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "SiteType",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Sites");
        }
    }
}
