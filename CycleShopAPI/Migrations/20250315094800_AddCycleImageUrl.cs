using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CycleShopAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCycleImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Cycles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Cycles");
        }
    }
}
