
using System;
using Cotacao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
#nullable disable
namespace Cotacao.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(CotacaoDbContext))]
    partial class CotacaoDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);
            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);
            modelBuilder.Entity("Cotacao.Domain.CotacaoB3", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<DateOnly>("DataPregao")
                        .HasColumnType("date");
                    b.Property<decimal>("PrecoAbertura")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");
                    b.Property<decimal>("PrecoFechamento")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");
                    b.Property<decimal>("PrecoMaximo")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");
                    b.Property<decimal>("PrecoMinimo")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");
                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");
                    b.HasKey("Id");
                    b.HasIndex("DataPregao")
                        .HasDatabaseName("IX_Cotacoes_DataPregao");
                    b.HasIndex("DataPregao", "Ticker")
                        .IsUnique()
                        .HasDatabaseName("IX_Cotacoes_DataPregao_Ticker");
                    b.ToTable("Cotacoes", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
