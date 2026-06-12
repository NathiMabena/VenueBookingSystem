using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenueBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venues_EventTypes_EventTypeId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Venues_EventTypeId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Venues");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Venues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTypeId",
                table: "Events",
                column: "EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EventTypes_EventTypeId",
                table: "Events",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventTypes_EventTypeId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventTypeId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Events");

            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Venues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_EventTypeId",
                table: "Venues",
                column: "EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_EventTypes_EventTypeId",
                table: "Venues",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeId");
        }
    }
}
