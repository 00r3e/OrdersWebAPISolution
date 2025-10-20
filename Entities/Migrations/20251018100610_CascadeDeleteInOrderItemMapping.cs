using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteInOrderItemMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReviews_OrderItems_OrderItemId",
                table: "OrderItemReviews");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReviews_OrderItems_OrderItemId",
                table: "OrderItemReviews",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "OrderItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReviews_OrderItems_OrderItemId",
                table: "OrderItemReviews");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReviews_OrderItems_OrderItemId",
                table: "OrderItemReviews",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "OrderItemId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
