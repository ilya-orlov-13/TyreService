using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class MakeCompletedWorkMasterIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedWorks_Masters_MasterId",
                table: "CompletedWorks");

            migrationBuilder.AlterColumn<int>(
                name: "MasterId",
                table: "CompletedWorks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedWorks_Masters_MasterId",
                table: "CompletedWorks",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "MasterId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedWorks_Masters_MasterId",
                table: "CompletedWorks");

            migrationBuilder.AlterColumn<int>(
                name: "MasterId",
                table: "CompletedWorks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedWorks_Masters_MasterId",
                table: "CompletedWorks",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "MasterId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
