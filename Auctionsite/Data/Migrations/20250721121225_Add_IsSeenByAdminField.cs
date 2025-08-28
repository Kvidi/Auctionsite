using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctionsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsSeenByAdminField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByAdmin",
                table: "Advertisements",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeenByAdmin",
                table: "Advertisements");
        }
    }
}
