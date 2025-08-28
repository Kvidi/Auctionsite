using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctionsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseColumnsToAdvertisement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PurchasedAt",
                table: "Advertisements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchasedByUserId",
                table: "Advertisements",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_PurchasedByUserId",
                table: "Advertisements",
                column: "PurchasedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_AspNetUsers_PurchasedByUserId",
                table: "Advertisements",
                column: "PurchasedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_AspNetUsers_PurchasedByUserId",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_PurchasedByUserId",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "PurchasedAt",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "PurchasedByUserId",
                table: "Advertisements");
        }
    }
}
