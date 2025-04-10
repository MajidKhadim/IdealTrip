using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdealTrip.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedHotelandCompletedIt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "UserHotelRoomBookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<float>(
                name: "Rating",
                table: "Transports",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AlterColumn<string>(
                name: "HotelDescription",
                table: "Hotels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<float>(
                name: "Rating",
                table: "Hotels",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Hotels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "UserHotelRoomBookings");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Transports");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Hotels");

            migrationBuilder.AlterColumn<string>(
                name: "HotelDescription",
                table: "Hotels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
