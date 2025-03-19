using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CycleShopAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameUnitPriceToPriceSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "OrderItems",
                newName: "PriceSnapshot");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "OrderItems",
                type: "numeric(10,2)",
                nullable: false,
                computedColumnSql: "\"Quantity\" * \"PriceSnapshot\"",
                stored: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldComputedColumnSql: "\"Quantity\" * \"UnitPrice\"",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceSnapshot",
                table: "OrderItems",
                newName: "UnitPrice");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "OrderItems",
                type: "numeric(10,2)",
                nullable: false,
                computedColumnSql: "\"Quantity\" * \"UnitPrice\"",
                stored: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldComputedColumnSql: "\"Quantity\" * \"PriceSnapshot\"",
                oldStored: true);
        }
    }
}
