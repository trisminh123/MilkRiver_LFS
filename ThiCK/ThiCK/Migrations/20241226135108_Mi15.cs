using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThiCK.Migrations
{
    /// <inheritdoc />
    public partial class Mi15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductQuantitíe",
                table: "ProductQuantitíe");

            migrationBuilder.RenameTable(
                name: "ProductQuantitíe",
                newName: "ProductQuantities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductQuantities",
                table: "ProductQuantities",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductQuantities",
                table: "ProductQuantities");

            migrationBuilder.RenameTable(
                name: "ProductQuantities",
                newName: "ProductQuantitíe");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductQuantitíe",
                table: "ProductQuantitíe",
                column: "Id");
        }
    }
}
