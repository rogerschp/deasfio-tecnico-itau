
using System;
using Clientes.Service.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
#nullable disable
namespace Clientes.Service.Migrations
{
    [DbContext(typeof(ClientesDbContext))]
    partial class ClientesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);
            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);
            modelBuilder.Entity("Clientes.Service.Infrastructure.Persistence.AporteEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<long>("ClienteId")
                        .HasColumnType("bigint");
                    b.Property<DateOnly>("DataAporte")
                        .HasColumnType("date");
                    b.Property<int>("Parcela")
                        .HasColumnType("int");
                    b.Property<decimal>("Valor")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");
                    b.HasKey("Id");
                    b.HasIndex("ClienteId");
                    b.ToTable("Aportes", (string)null);
                });
            modelBuilder.Entity("Clientes.Service.Infrastructure.Persistence.ClienteEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<bool>("Ativo")
                        .HasColumnType("tinyint(1)");
                    b.Property<long?>("ContaGraficaId")
                        .HasColumnType("bigint");
                    b.Property<string>("Cpf")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("varchar(11)");
                    b.Property<DateTime>("DataAdesao")
                        .HasColumnType("datetime(6)");
                    b.Property<DateTime?>("DataSaida")
                        .HasColumnType("datetime(6)");
                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");
                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");
                    b.Property<decimal>("ValorMensal")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");
                    b.HasKey("Id");
                    b.HasIndex("Cpf")
                        .IsUnique();
                    b.ToTable("Clientes", (string)null);
                });
            modelBuilder.Entity("Clientes.Service.Infrastructure.Persistence.ContaGraficaEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<long>("ClienteId")
                        .HasColumnType("bigint");
                    b.Property<DateTime>("DataCriacao")
                        .HasColumnType("datetime(6)");
                    b.Property<string>("NumeroConta")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");
                    b.Property<string>("Tipo")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");
                    b.HasKey("Id");
                    b.HasIndex("NumeroConta")
                        .IsUnique();
                    b.ToTable("ContasGraficas", (string)null);
                });
            modelBuilder.Entity("Clientes.Service.Infrastructure.Persistence.CustodiaFilhoteEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<long>("ContaGraficaId")
                        .HasColumnType("bigint");
                    b.Property<decimal>("PrecoMedio")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");
                    b.Property<int>("Quantidade")
                        .HasColumnType("int");
                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("varchar(12)");
                    b.HasKey("Id");
                    b.HasIndex("ContaGraficaId", "Ticker")
                        .IsUnique();
                    b.ToTable("CustodiasFilhote", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
