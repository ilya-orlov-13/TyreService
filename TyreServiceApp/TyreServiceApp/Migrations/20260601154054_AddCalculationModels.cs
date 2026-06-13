using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TyreServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculationModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ClientTotal",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConsumablesCost",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                table: "Orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CarClassId",
                table: "Cars",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarClasses",
                columns: table => new
                {
                    CarClassId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseTariff = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarClasses", x => x.CarClassId);
                });

            migrationBuilder.CreateTable(
                name: "ComplexityCoefficients",
                columns: table => new
                {
                    ComplexityCoefficientId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Factor = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexityCoefficients", x => x.ComplexityCoefficientId);
                });

            migrationBuilder.CreateTable(
                name: "Consumables",
                columns: table => new
                {
                    ConsumableId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    SellPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumables", x => x.ConsumableId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTariffs",
                columns: table => new
                {
                    ServiceTariffId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceCode = table.Column<int>(type: "integer", nullable: false),
                    CarClassId = table.Column<int>(type: "integer", nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MasterSharePercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTariffs", x => x.ServiceTariffId);
                    table.ForeignKey(
                        name: "FK_ServiceTariffs_CarClasses_CarClassId",
                        column: x => x.CarClassId,
                        principalTable: "CarClasses",
                        principalColumn: "CarClassId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceTariffs_Services_ServiceCode",
                        column: x => x.ServiceCode,
                        principalTable: "Services",
                        principalColumn: "ServiceCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderComplexities",
                columns: table => new
                {
                    OrderComplexityId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderNumber = table.Column<int>(type: "integer", nullable: false),
                    ComplexityCoefficientId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderComplexities", x => x.OrderComplexityId);
                    table.ForeignKey(
                        name: "FK_OrderComplexities_ComplexityCoefficients_ComplexityCoeffici~",
                        column: x => x.ComplexityCoefficientId,
                        principalTable: "ComplexityCoefficients",
                        principalColumn: "ComplexityCoefficientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderComplexities_Orders_OrderNumber",
                        column: x => x.OrderNumber,
                        principalTable: "Orders",
                        principalColumn: "OrderNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderConsumables",
                columns: table => new
                {
                    OrderConsumableId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderNumber = table.Column<int>(type: "integer", nullable: false),
                    ConsumableId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderConsumables", x => x.OrderConsumableId);
                    table.ForeignKey(
                        name: "FK_OrderConsumables_Consumables_ConsumableId",
                        column: x => x.ConsumableId,
                        principalTable: "Consumables",
                        principalColumn: "ConsumableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderConsumables_Orders_OrderNumber",
                        column: x => x.OrderNumber,
                        principalTable: "Orders",
                        principalColumn: "OrderNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_CarClassId",
                table: "Cars",
                column: "CarClassId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderComplexities_ComplexityCoefficientId",
                table: "OrderComplexities",
                column: "ComplexityCoefficientId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderComplexities_OrderNumber",
                table: "OrderComplexities",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OrderConsumables_ConsumableId",
                table: "OrderConsumables",
                column: "ConsumableId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderConsumables_OrderNumber",
                table: "OrderConsumables",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTariffs_CarClassId",
                table: "ServiceTariffs",
                column: "CarClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTariffs_ServiceCode_CarClassId",
                table: "ServiceTariffs",
                columns: new[] { "ServiceCode", "CarClassId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_CarClasses_CarClassId",
                table: "Cars",
                column: "CarClassId",
                principalTable: "CarClasses",
                principalColumn: "CarClassId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cars_CarClasses_CarClassId",
                table: "Cars");

            migrationBuilder.DropTable(
                name: "OrderComplexities");

            migrationBuilder.DropTable(
                name: "OrderConsumables");

            migrationBuilder.DropTable(
                name: "ServiceTariffs");

            migrationBuilder.DropTable(
                name: "ComplexityCoefficients");

            migrationBuilder.DropTable(
                name: "Consumables");

            migrationBuilder.DropTable(
                name: "CarClasses");

            migrationBuilder.DropIndex(
                name: "IX_Cars_CarClassId",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "ClientTotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ConsumablesCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CarClassId",
                table: "Cars");
        }
    }
}
