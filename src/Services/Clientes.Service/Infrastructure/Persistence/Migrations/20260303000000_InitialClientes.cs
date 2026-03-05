using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Clientes.Service.Infrastructure.Persistence.Migrations;
public partial class InitialClientes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase().Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.CreateTable(
            name: "Clientes",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                Cpf = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                ValorMensal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DataAdesao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                DataSaida = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                ContaGraficaId = table.Column<long>(type: "bigint", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Clientes", x => x.Id))
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `Clientes` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");
        migrationBuilder.CreateIndex(name: "IX_Clientes_Cpf", table: "Clientes", column: "Cpf", unique: true);
        migrationBuilder.CreateTable(
            name: "ContasGraficas",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                NumeroConta = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                Tipo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                ClienteId = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ContasGraficas", x => x.Id))
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `ContasGraficas` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");
        migrationBuilder.CreateIndex(name: "IX_ContasGraficas_NumeroConta", table: "ContasGraficas", column: "NumeroConta", unique: true);
        migrationBuilder.CreateTable(
            name: "CustodiasFilhote",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                ContaGraficaId = table.Column<long>(type: "bigint", nullable: false),
                Ticker = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                Quantidade = table.Column<int>(type: "int", nullable: false),
                PrecoMedio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CustodiasFilhote", x => x.Id))
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `CustodiasFilhote` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");
        migrationBuilder.CreateIndex(name: "IX_CustodiasFilhote_ContaGraficaId_Ticker", table: "CustodiasFilhote", columns: new[] { "ContaGraficaId", "Ticker" }, unique: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("CustodiasFilhote");
        migrationBuilder.DropTable("ContasGraficas");
        migrationBuilder.DropTable("Clientes");
    }
}
