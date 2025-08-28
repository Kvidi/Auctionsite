using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctionsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHeadImageURL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeadImageURL",
                table: "Advertisements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeadImageURL",
                table: "Advertisements",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");
        }
    }
}
