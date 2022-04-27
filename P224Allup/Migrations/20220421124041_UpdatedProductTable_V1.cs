using Microsoft.EntityFrameworkCore.Migrations;

namespace P224Allup.Migrations
{
    public partial class UpdatedProductTable_V1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBestseller",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNewArrival",
                table: "Products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBestseller",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsNewArrival",
                table: "Products");
        }
    }
}
