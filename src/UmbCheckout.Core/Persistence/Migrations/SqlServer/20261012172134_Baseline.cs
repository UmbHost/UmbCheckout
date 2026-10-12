using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmbCheckout.Core.Persistence.Migrations.SqlServer
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SuccessPageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CancelPageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoreBasketInCookie = table.Column<bool>(type: "bit", nullable: false),
                    StoreBasketInDatabase = table.Column<bool>(type: "bit", nullable: false),
                    BasketInDatabaseExpiry = table.Column<int>(type: "int", nullable: false),
                    BasketInCookieExpiry = table.Column<int>(type: "int", nullable: false),
                    EnableShipping = table.Column<bool>(type: "bit", nullable: false)
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
