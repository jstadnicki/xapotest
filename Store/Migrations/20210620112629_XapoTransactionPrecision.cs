using Microsoft.EntityFrameworkCore.Migrations;

namespace Store.Migrations
{
    public partial class XapoTransactionPrecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Charge",
                table: "XapoTransaction",
                type: "decimal(12,8)",
                precision: 12,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,8)",
                oldPrecision: 10,
                oldScale: 8);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Charge",
                table: "XapoTransaction",
                type: "decimal(10,8)",
                precision: 10,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,8)",
                oldPrecision: 12,
                oldScale: 8);
        }
    }
}
