using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmbCheckout.Core.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UmbCheckoutConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(type: "TEXT", nullable: true),
                    CurrencyCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true, collation: "NOCASE"),
                    SuccessPageUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, collation: "NOCASE"),
                    CancelPageUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, collation: "NOCASE"),
                    StoreBasketInCookie = table.Column<bool>(type: "INTEGER", nullable: false),
                    StoreBasketInDatabase = table.Column<bool>(type: "INTEGER", nullable: false),
                    BasketInDatabaseExpiry = table.Column<int>(type: "INTEGER", nullable: false),
                    BasketInCookieExpiry = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableShipping = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmbCheckoutConfiguration", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UmbCheckoutConfiguration");
        }
    }
}
