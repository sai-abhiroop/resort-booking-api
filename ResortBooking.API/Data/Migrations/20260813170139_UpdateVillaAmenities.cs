using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResortBooking.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVillaAmenities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDtae",
                table: "VillaAmenities",
                newName: "UpdatedDate");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "VillaAmenities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "VillaAmenities",
                newName: "UpdatedDtae");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "VillaAmenities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
