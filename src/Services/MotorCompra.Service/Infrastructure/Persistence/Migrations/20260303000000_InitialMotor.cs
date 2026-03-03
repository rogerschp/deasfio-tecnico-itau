using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotorCompra.Service.Infrastructure.Persistence.Migrations;

public partial class InitialMotor : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "ExecucoesCompra",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                DataReferencia = table.Column<DateOnly>(type: "date", nullable: false),
                DataExecucao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                TotalConsolidado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                TotalClientes = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ExecucoesCompra", x => x.Id))
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `ExecucoesCompra` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");

        migrationBuilder.CreateIndex(
            name: "IX_ExecucoesCompra_DataReferencia",
            table: "ExecucoesCompra",
            column: "DataReferencia",
            unique: true);

        migrationBuilder.CreateTable(
            name: "CustodiaMaster",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                Ticker = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Quantidade = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CustodiaMaster", x => x.Id))
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `CustodiaMaster` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");

        migrationBuilder.CreateIndex(
            name: "IX_CustodiaMaster_Ticker",
            table: "CustodiaMaster",
            column: "Ticker",
            unique: true);

        migrationBuilder.CreateTable(
            name: "OrdensCompra",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                ExecucaoCompraId = table.Column<long>(type: "bigint", nullable: false),
                Ticker = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                QuantidadeTotal = table.Column<int>(type: "int", nullable: false),
                PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                ValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                DetalhesJson = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrdensCompra", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrdensCompra_ExecucoesCompra_ExecucaoCompraId",
                    column: x => x.ExecucaoCompraId,
                    principalTable: "ExecucoesCompra",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `OrdensCompra` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");

        migrationBuilder.CreateIndex(
            name: "IX_OrdensCompra_ExecucaoCompraId",
            table: "OrdensCompra",
            column: "ExecucaoCompraId");

        migrationBuilder.CreateTable(
            name: "Distribuicoes",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                ExecucaoCompraId = table.Column<long>(type: "bigint", nullable: false),
                ClienteId = table.Column<long>(type: "bigint", nullable: false),
                Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Cpf = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ValorAporte = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                AtivosJson = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Distribuicoes", x => x.Id);
                table.ForeignKey(
                    name: "FK_Distribuicoes_ExecucoesCompra_ExecucaoCompraId",
                    column: x => x.ExecucaoCompraId,
                    principalTable: "ExecucoesCompra",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `Distribuicoes` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");

        migrationBuilder.CreateIndex(
            name: "IX_Distribuicoes_ExecucaoCompraId",
            table: "Distribuicoes",
            column: "ExecucaoCompraId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CustodiaMaster");
        migrationBuilder.DropTable(name: "OrdensCompra");
        migrationBuilder.DropTable(name: "Distribuicoes");
        migrationBuilder.DropTable(name: "ExecucoesCompra");
    }
}
