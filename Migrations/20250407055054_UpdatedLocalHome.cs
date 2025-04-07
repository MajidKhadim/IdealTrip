using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdealTrip.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedLocalHome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "UserLocalHomesBookings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "UserLocalHomesBookings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AlterColumn<DateOnly>(
                name: "AvailableTo",
                table: "LocalHomes",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "AvailableFrom",
                table: "LocalHomes",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRooms",
                table: "LocalHomes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "UserLocalHomesBookings");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "UserLocalHomesBookings");

            migrationBuilder.DropColumn(
                name: "NumberOfRooms",
                table: "LocalHomes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AvailableTo",
                table: "LocalHomes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AvailableFrom",
                table: "LocalHomes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
