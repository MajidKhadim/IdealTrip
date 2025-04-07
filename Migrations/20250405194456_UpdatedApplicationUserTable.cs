using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdealTrip.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedApplicationUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BounceReason",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BounceReason",
                table: "AspNetUsers");
        }
    }
}
