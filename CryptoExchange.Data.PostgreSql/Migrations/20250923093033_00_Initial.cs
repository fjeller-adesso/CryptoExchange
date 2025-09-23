using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoExchange.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class _00_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "order_books");

            migrationBuilder.CreateTable(
                name: "exchange",
                schema: "order_books",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    available_crypto = table.Column<decimal>(type: "numeric", nullable: false),
                    available_euro = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("exchange_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exchange_order",
                schema: "order_books",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exchange_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("exchange_order_pkey", x => x.id);
                    table.ForeignKey(
                        name: "orders_exchanges",
                        column: x => x.exchange_id,
                        principalSchema: "order_books",
                        principalTable: "exchange",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "fki_orders_exchanges",
                schema: "order_books",
                table: "exchange_order",
                column: "exchange_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exchange_order",
                schema: "order_books");

            migrationBuilder.DropTable(
                name: "exchange",
                schema: "order_books");
        }
    }
}
