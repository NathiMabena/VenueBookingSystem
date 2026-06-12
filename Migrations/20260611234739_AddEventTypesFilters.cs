using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VenueBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypesFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Venues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "EventTypeId", "TypeName" },
                values: new object[,]
                {
                    { 1, "Conference" },
                    { 2, "Concert" },
                    { 3, "Wedding" },
                    { 4, "Workshop" }
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venues_EventTypes_EventTypeId",
                table: "Venues");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_Venues_EventTypeId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Venues");
        }
    }
}
