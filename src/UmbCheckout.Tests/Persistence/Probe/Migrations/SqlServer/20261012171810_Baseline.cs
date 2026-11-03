using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmbCheckout.Tests.Persistence.Probe.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProbeRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(20,9)", precision: 20, scale: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProbeRows", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProbeRows");
        }
    }
}
