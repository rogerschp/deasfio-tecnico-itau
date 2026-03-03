using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MotorCompra.Service.Infrastructure.Persistence;

#nullable disable

namespace MotorCompra.Service.Infrastructure.Persistence.Migrations;

[DbContext(typeof(MotorDbContext))]
partial class MotorDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.ExecucaoCompraEntity", b =>
        {
            b.Property<long>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("bigint");
            b.Property<DateOnly>("DataReferencia")
                .HasColumnType("date");
            b.Property<DateTime>("DataExecucao")
                .HasColumnType("datetime(6)");
            b.Property<decimal>("TotalConsolidado")
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");
            b.Property<int>("TotalClientes")
                .HasColumnType("int");
            b.HasKey("Id");
            b.HasIndex("DataReferencia")
                .IsUnique();
            b.ToTable("ExecucoesCompra");
        });

        modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.OrdemCompraEntity", b =>
        {
            b.Property<long>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("bigint");
            b.Property<long>("ExecucaoCompraId")
                .HasColumnType("bigint");
            b.Property<string>("Ticker")
                .IsRequired()
                .HasMaxLength(12)
                .HasColumnType("varchar(12)");
            b.Property<int>("QuantidadeTotal")
                .HasColumnType("int");
            b.Property<decimal>("PrecoUnitario")
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");
            b.Property<decimal>("ValorTotal")
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");
            b.Property<string>("DetalhesJson")
                .IsRequired()
                .HasColumnType("longtext");
            b.HasKey("Id");
            b.HasIndex("ExecucaoCompraId");
            b.ToTable("OrdensCompra");
        });

        modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.DistribuicaoEntity", b =>
        {
            b.Property<long>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("bigint");
            b.Property<long>("ExecucaoCompraId")
                .HasColumnType("bigint");
            b.Property<long>("ClienteId")
                .HasColumnType("bigint");
            b.Property<string>("Nome")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");
            b.Property<string>("Cpf")
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnType("varchar(11)");
            b.Property<decimal>("ValorAporte")
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");
            b.Property<string>("AtivosJson")
                .IsRequired()
                .HasColumnType("longtext");
            b.HasKey("Id");
            b.HasIndex("ExecucaoCompraId");
            b.ToTable("Distribuicoes");
        });

        modelBuilder.Entity("MotorCompra.Service.Infrastructure.Persistence.CustodiaMasterEntity", b =>
        {
            b.Property<long>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("bigint");
            b.Property<string>("Ticker")
                .IsRequired()
                .HasMaxLength(12)
                .HasColumnType("varchar(12)");
            b.Property<int>("Quantidade")
                .HasColumnType("int");
            b.HasKey("Id");
            b.HasIndex("Ticker")
                .IsUnique();
            b.ToTable("CustodiaMaster");
        });
    }
}
