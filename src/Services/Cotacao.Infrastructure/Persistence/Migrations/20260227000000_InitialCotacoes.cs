using System;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Cotacao.Infrastructure.Persistence.Migrations;

public partial class InitialCotacoes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.CreateTable(
            name: "Cotacoes",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                DataPregao = table.Column<DateOnly>(type: "date", nullable: false),
                Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PrecoAbertura = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                PrecoFechamento = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                PrecoMaximo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                PrecoMinimo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cotacoes", x => x.Id);
            },
            comment: "Cotações B3 (COTAHIST) por data e ticker")
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.Sql("ALTER TABLE `Cotacoes` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;");
        migrationBuilder.CreateIndex(
            name: "IX_Cotacoes_DataPregao",
            table: "Cotacoes",
            column: "DataPregao");
        migrationBuilder.CreateIndex(
            name: "IX_Cotacoes_DataPregao_Ticker",
            table: "Cotacoes",
            columns: new[] { "DataPregao", "Ticker" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Cotacoes");
    }
}
