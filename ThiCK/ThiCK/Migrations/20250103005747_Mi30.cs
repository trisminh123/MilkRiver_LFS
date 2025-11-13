using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThiCK.Migrations
{
    /// <inheritdoc />
    public partial class Mi30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Wishlists");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Compares");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Wishlists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Compares",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
