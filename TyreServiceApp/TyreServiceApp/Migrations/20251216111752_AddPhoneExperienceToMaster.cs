using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneExperienceToMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedWorks_Masters_MasterId",
                table: "CompletedWorks");

            migrationBuilder.DropForeignKey(
                name: "FK_CompletedWorks_Services_ServiceCode",
                table: "CompletedWorks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Orders",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<int>(
                name: "MasterId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_MasterId",
                table: "Orders",
                column: "MasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedWorks_Masters_MasterId",
                table: "CompletedWorks",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "MasterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedWorks_Services_ServiceCode",
                table: "CompletedWorks",
                column: "ServiceCode",
                principalTable: "Services",
                principalColumn: "ServiceCode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Masters_MasterId",
                table: "Orders",
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

            migrationBuilder.DropForeignKey(
                name: "FK_CompletedWorks_Services_ServiceCode",
                table: "CompletedWorks");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Masters_MasterId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_MasterId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "Orders");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Orders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedWorks_Masters_MasterId",
                table: "CompletedWorks",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "MasterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedWorks_Services_ServiceCode",
                table: "CompletedWorks",
                column: "ServiceCode",
                principalTable: "Services",
                principalColumn: "ServiceCode",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
