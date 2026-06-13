using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffPositionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffPositions",
                columns: table => new
                {
                    StaffPositionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPositions", x => x.StaffPositionId);
                });

            // Move existing staff positions from Positions to StaffPositions
            migrationBuilder.Sql(@"
                INSERT INTO ""StaffPositions"" (""Name"")
                SELECT ""Name"" FROM ""Positions"" WHERE ""Category"" = 1
            ");

            // Remove old staff positions from Positions
            migrationBuilder.Sql(@"
                DELETE FROM ""Positions"" WHERE ""Category"" = 1
            ");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Positions");

            migrationBuilder.AddColumn<int>(
                name: "StaffPositionId",
                table: "AdminUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_StaffPositionId",
                table: "AdminUsers",
                column: "StaffPositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminUsers_StaffPositions_StaffPositionId",
                table: "AdminUsers",
                column: "StaffPositionId",
                principalTable: "StaffPositions",
                principalColumn: "StaffPositionId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore Category column
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Positions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Restore staff positions back to Positions
            migrationBuilder.Sql(@"
                INSERT INTO ""Positions"" (""Name"", ""Category"")
                SELECT ""Name"", 1 FROM ""StaffPositions""
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_AdminUsers_StaffPositions_StaffPositionId",
                table: "AdminUsers");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_StaffPositionId",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "StaffPositionId",
                table: "AdminUsers");

            migrationBuilder.DropTable(
                name: "StaffPositions");
        }
    }
}
