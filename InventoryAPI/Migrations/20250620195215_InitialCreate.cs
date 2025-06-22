using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "Name", "Price", "Stock", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Electronics", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Laptop HP Pavilion 15.6 pulgadas", "Laptop HP", 899.99m, 10, null },
                    { 2, "Accessories", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Mouse inalámbrico ergonómico", "Mouse Inalámbrico", 25.99m, 50, null },
                    { 3, "Accessories", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Teclado mecánico RGB retroiluminado", "Teclado Mecánico", 79.99m, 25, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
