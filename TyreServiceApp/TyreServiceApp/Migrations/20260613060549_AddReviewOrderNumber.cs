using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewOrderNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerReviews_CustomerId",
                table: "CustomerReviews");

            migrationBuilder.AddColumn<int>(
                name: "OrderNumber",
                table: "CustomerReviews",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReviews_CustomerId",
                table: "CustomerReviews",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReviews_OrderNumber",
                table: "CustomerReviews",
                column: "OrderNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerReviews_Orders_OrderNumber",
                table: "CustomerReviews",
                column: "OrderNumber",
                principalTable: "Orders",
                principalColumn: "OrderNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerReviews_Orders_OrderNumber",
                table: "CustomerReviews");

            migrationBuilder.DropIndex(
                name: "IX_CustomerReviews_CustomerId",
                table: "CustomerReviews");

            migrationBuilder.DropIndex(
                name: "IX_CustomerReviews_OrderNumber",
                table: "CustomerReviews");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "CustomerReviews");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReviews_CustomerId",
                table: "CustomerReviews",
                column: "CustomerId",
                unique: true);
        }
    }
}
