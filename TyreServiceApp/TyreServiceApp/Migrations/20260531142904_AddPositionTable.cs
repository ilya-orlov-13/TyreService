using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                });

            // Seed default positions
            migrationBuilder.InsertData(
                table: "Positions",
                columns: new[] { "PositionId", "Name" },
                values: new object[,]
                {
                    { 1, "Шиномонтажник" },
                    { 2, "Балансировщик" },
                    { 3, "Мастер-приёмщик" },
                    { 4, "Старший механик" },
                    { 5, "Помощник мастера" }
                });

            // Assign existing masters to "Шиномонтажник" (PositionId = 1)
            // First add column with a default, then alter to remove default
            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "Masters",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Masters");

            migrationBuilder.CreateIndex(
                name: "IX_Masters_PositionId",
                table: "Masters",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Masters_Positions_PositionId",
                table: "Masters",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "PositionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Masters_Positions_PositionId",
                table: "Masters");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Masters_PositionId",
                table: "Masters");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Masters");

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Masters",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
