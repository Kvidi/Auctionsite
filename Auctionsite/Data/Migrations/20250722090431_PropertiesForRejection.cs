using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctionsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class PropertiesForRejection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "Advertisements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Advertisements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Advertisements");
        }
    }
}
