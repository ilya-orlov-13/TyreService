using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerUserClientCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerUsers_Clients_ClientId",
                table: "CustomerUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerUsers_Clients_ClientId",
                table: "CustomerUsers",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerUsers_Clients_ClientId",
                table: "CustomerUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerUsers_Clients_ClientId",
                table: "CustomerUsers",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId");
        }
    }
}
