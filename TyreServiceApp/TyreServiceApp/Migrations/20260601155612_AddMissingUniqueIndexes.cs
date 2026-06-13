using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderConsumables_OrderNumber",
                table: "OrderConsumables");

            migrationBuilder.DropIndex(
                name: "IX_OrderComplexities_OrderNumber",
                table: "OrderComplexities");

            migrationBuilder.CreateIndex(
                name: "IX_OrderConsumables_OrderNumber_ConsumableId",
                table: "OrderConsumables",
                columns: new[] { "OrderNumber", "ConsumableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderComplexities_OrderNumber_ComplexityCoefficientId",
                table: "OrderComplexities",
                columns: new[] { "OrderNumber", "ComplexityCoefficientId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderConsumables_OrderNumber_ConsumableId",
                table: "OrderConsumables");

            migrationBuilder.DropIndex(
                name: "IX_OrderComplexities_OrderNumber_ComplexityCoefficientId",
                table: "OrderComplexities");

            migrationBuilder.CreateIndex(
                name: "IX_OrderConsumables_OrderNumber",
                table: "OrderConsumables",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OrderComplexities_OrderNumber",
                table: "OrderComplexities",
                column: "OrderNumber");
        }
    }
}
