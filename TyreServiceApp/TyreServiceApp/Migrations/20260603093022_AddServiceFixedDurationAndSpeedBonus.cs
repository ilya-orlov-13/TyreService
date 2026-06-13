using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceFixedDurationAndSpeedBonus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FixedDurationMin",
                table: "Services",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeSavedMin",
                table: "CompletedWorks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SpeedBonuses",
                columns: table => new
                {
                    SpeedBonusId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MasterId = table.Column<int>(type: "integer", nullable: false),
                    OrderNumber = table.Column<int>(type: "integer", nullable: false),
                    WorkId = table.Column<int>(type: "integer", nullable: false),
                    TimeSavedMin = table.Column<int>(type: "integer", nullable: false),
                    BonusAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeedBonuses", x => x.SpeedBonusId);
                    table.ForeignKey(
                        name: "FK_SpeedBonuses_Masters_MasterId",
                        column: x => x.MasterId,
                        principalTable: "Masters",
                        principalColumn: "MasterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeedBonuses_Orders_OrderNumber",
                        column: x => x.OrderNumber,
                        principalTable: "Orders",
                        principalColumn: "OrderNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpeedBonuses_MasterId",
                table: "SpeedBonuses",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeedBonuses_OrderNumber",
                table: "SpeedBonuses",
                column: "OrderNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpeedBonuses");

            migrationBuilder.DropColumn(
                name: "FixedDurationMin",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "TimeSavedMin",
                table: "CompletedWorks");
        }
    }
}
