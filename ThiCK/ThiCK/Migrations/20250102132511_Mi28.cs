using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThiCK.Migrations
{
    /// <inheritdoc />
    public partial class Mi28 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddQuantity",
                table: "ProductQuantities");

            migrationBuilder.RenameColumn(
                name: "SubtractQuantity",
                table: "ProductQuantities",
                newName: "Quantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "ProductQuantities",
                newName: "SubtractQuantity");

            migrationBuilder.AddColumn<int>(
                name: "AddQuantity",
                table: "ProductQuantities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
