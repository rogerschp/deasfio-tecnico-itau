
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MotorCompra.Service.Infrastructure.Persistence;
#nullable disable
namespace MotorCompra.Service.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(MotorDbContext))]
    partial class MotorDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);
            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);
            modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.CustodiaMasterEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<int>("Quantidade")
                        .HasColumnType("int");
                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("varchar(12)");
                    b.HasKey("Id");
                    b.HasIndex("Ticker")
                        .IsUnique();
                    b.ToTable("CustodiaMaster", (string)null);
                });
            modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.DistribuicaoEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<string>("AtivosJson")
                        .IsRequired()
                        .HasColumnType("longtext");
                    b.Property<long>("ClienteId")
                        .HasColumnType("bigint");
                    b.Property<string>("Cpf")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("varchar(11)");
                    b.Property<long>("ExecucaoCompraId")
                        .HasColumnType("bigint");
                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");
                    b.Property<decimal>("ValorAporte")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");
                    b.HasKey("Id");
                    b.HasIndex("ExecucaoCompraId");
                    b.ToTable("Distribuicoes", (string)null);
                });
            modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.ExecucaoCompraEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<DateTime>("DataExecucao")
                        .HasColumnType("datetime(6)");
                    b.Property<DateOnly>("DataReferencia")
                        .HasColumnType("date");
                    b.Property<int>("TotalClientes")
                        .HasColumnType("int");
                    b.Property<decimal>("TotalConsolidado")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");
                    b.HasKey("Id");
                    b.HasIndex("DataReferencia")
                        .IsUnique();
                    b.ToTable("ExecucoesCompra", (string)null);
                });
            modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.OrdemCompraEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<string>("DetalhesJson")
                        .IsRequired()
                        .HasColumnType("longtext");
                    b.Property<long>("ExecucaoCompraId")
                        .HasColumnType("bigint");
                    b.Property<decimal>("PrecoUnitario")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");
                    b.Property<int>("QuantidadeTotal")
                        .HasColumnType("int");
                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("varchar(12)");
                    b.Property<decimal>("ValorTotal")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");
                    b.HasKey("Id");
                    b.HasIndex("ExecucaoCompraId");
                    b.ToTable("OrdensCompra", (string)null);
                });
            modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.DistribuicaoEntity", b =>
                {
                    b.HasOne("MotorCompra.Service.Infrastructure.Persistence.ExecucaoCompraEntity", "ExecucaoCompra")
                        .WithMany("Distribuicoes")
                        .HasForeignKey("ExecucaoCompraId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("ExecucaoCompra");
                });
            modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.OrdemCompraEntity", b =>
                {
                    b.HasOne("MotorCompra.Service.Infrastructure.Persistence.ExecucaoCompraEntity", "ExecucaoCompra")
                        .WithMany("Ordens")
                        .HasForeignKey("ExecucaoCompraId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("ExecucaoCompra");
                });
            modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.ExecucaoCompraEntity", b =>
                {
                    b.Navigation("Distribuicoes");
                    b.Navigation("Ordens");
                });
#pragma warning restore 612, 618
        }
    }
}
