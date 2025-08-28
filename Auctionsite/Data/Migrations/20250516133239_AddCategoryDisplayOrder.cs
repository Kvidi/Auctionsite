using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctionsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Advertisements",
                newName: "PickupLocation");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "CategoryForAdvertisements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuctionEndDate",
                table: "Advertisements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AvailableForPickup",
                table: "Advertisements",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "CategoryForAdvertisements");

            migrationBuilder.DropColumn(
                name: "AuctionEndDate",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "AvailableForPickup",
                table: "Advertisements");

            migrationBuilder.RenameColumn(
                name: "PickupLocation",
                table: "Advertisements",
                newName: "Location");
        }
    }
}
