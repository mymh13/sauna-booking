using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaunaBookingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameAndTypeToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Bookings");
        }
    }
}
