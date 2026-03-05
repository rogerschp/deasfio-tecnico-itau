
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Rebalanceamento.Service.Infrastructure.Persistence;
#nullable disable
namespace Rebalanceamento.Service.Migrations
{
    [DbContext(typeof(RebalanceamentoDbContext))]
    partial class RebalanceamentoDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);
            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);
            modelBuilder.Entity("Rebalanceamento.Service.Infrastructure.Persistence.VendaRebalanceamentoEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<long>("ClienteId")
                        .HasColumnType("bigint");
                    b.Property<string>("Cpf")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");
                    b.Property<DateTime>("DataExecucao")
                        .HasColumnType("datetime(6)");
                    b.Property<decimal>("Lucro")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");
                    b.Property<decimal>("PrecoMedio")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");
                    b.Property<decimal>("PrecoVenda")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");
                    b.Property<int>("Quantidade")
                        .HasColumnType("int");
                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");
                    b.HasKey("Id");
                    b.HasIndex("ClienteId", "DataExecucao");
                    b.ToTable("vendas_rebalanceamento", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
