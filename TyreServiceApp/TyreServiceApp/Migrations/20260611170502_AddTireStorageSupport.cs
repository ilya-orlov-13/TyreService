using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTireStorageSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Cars_CarId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Tires_Cars_CarId",
                table: "Tires");

            migrationBuilder.AlterColumn<int>(
                name: "CarId",
                table: "Tires",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Tires",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CarId",
                table: "Orders",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "TireId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tires_ClientId",
                table: "Tires",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TireId",
                table: "Orders",
                column: "TireId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Cars_CarId",
                table: "Orders",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "CarId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Tires_TireId",
                table: "Orders",
                column: "TireId",
                principalTable: "Tires",
                principalColumn: "TireId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tires_Cars_CarId",
                table: "Tires",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "CarId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tires_Clients_ClientId",
                table: "Tires",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Cars_CarId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Tires_TireId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Tires_Cars_CarId",
                table: "Tires");

            migrationBuilder.DropForeignKey(
                name: "FK_Tires_Clients_ClientId",
                table: "Tires");

            migrationBuilder.DropIndex(
                name: "IX_Tires_ClientId",
                table: "Tires");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TireId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Tires");

            migrationBuilder.DropColumn(
                name: "TireId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "CarId",
                table: "Tires",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CarId",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Cars_CarId",
                table: "Orders",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "CarId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tires_Cars_CarId",
                table: "Tires",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "CarId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
