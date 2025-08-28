using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctionsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventType",
                table: "Bids",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Bids");
        }
    }
}
