using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Rebalanceamento.Service.Infrastructure.Persistence.Migrations;
public partial class InitialRebalanceamento : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.CreateTable(
            name: "vendas_rebalanceamento",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                ClienteId = table.Column<long>(type: "bigint", nullable: false),
                Cpf = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Ticker = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Quantidade = table.Column<int>(type: "int", nullable: false),
                PrecoVenda = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                PrecoMedio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                Lucro = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                DataExecucao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_vendas_rebalanceamento", x => x.Id))
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `vendas_rebalanceamento` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");
        migrationBuilder.CreateIndex(
            name: "IX_vendas_rebalanceamento_ClienteId_DataExecucao",
            table: "vendas_rebalanceamento",
            columns: new[] { "ClienteId", "DataExecucao" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "vendas_rebalanceamento");
    }
}
