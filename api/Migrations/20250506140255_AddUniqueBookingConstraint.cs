using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaunaBookingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueBookingConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Date_StartTime",
                table: "Bookings",
                columns: new[] { "Date", "StartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_Date_StartTime",
                table: "Bookings");
        }
    }
}
