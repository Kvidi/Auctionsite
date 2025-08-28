using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctionsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewAdImageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMain",
                table: "AdvertisementImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "AdvertisementImages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMain",
                table: "AdvertisementImages");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "AdvertisementImages");
        }
    }
}
