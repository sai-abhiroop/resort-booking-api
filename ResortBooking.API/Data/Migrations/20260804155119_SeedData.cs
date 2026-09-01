using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResortBooking.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "Villa");

            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "Villa",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Villa",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Villa");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Villa",
                newName: "UpdateDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Villa",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
