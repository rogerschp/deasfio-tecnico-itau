using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Clientes.Service.Infrastructure.Persistence.Migrations;
public partial class AddAportes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Aportes",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                ClienteId = table.Column<long>(type: "bigint", nullable: false),
                DataAporte = table.Column<DateOnly>(type: "date", nullable: false),
                Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Parcela = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Aportes", x => x.Id))
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `Aportes` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");
        migrationBuilder.CreateIndex(name: "IX_Aportes_ClienteId", table: "Aportes", column: "ClienteId");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Aportes");
    }
}
