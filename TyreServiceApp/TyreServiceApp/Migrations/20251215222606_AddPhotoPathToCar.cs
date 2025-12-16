using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoPathToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Cars",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Cars");
        }
    }
}
