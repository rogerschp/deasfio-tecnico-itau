
using System;
using Admin.Service.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
#nullable disable
namespace Admin.Service.Migrations
{
    [DbContext(typeof(AdminDbContext))]
    partial class AdminDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);
            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);
            modelBuilder.Entity("Admin.Service.Infrastructure.Persistence.CestaEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<bool>("Ativa")
                        .HasColumnType("tinyint(1)");
                    b.Property<DateTime>("DataCriacao")
                        .HasColumnType("datetime(6)");
                    b.Property<DateTime?>("DataDesativacao")
                        .HasColumnType("datetime(6)");
                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");
                    b.HasKey("Id");
                    b.HasIndex("Ativa")
                        .HasFilter("Ativa = 1");
                    b.ToTable("Cestas", (string)null);
                });
            modelBuilder.Entity("Admin.Service.Infrastructure.Persistence.ItemCestaEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");
                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));
                    b.Property<long>("CestaId")
                        .HasColumnType("bigint");
                    b.Property<decimal>("Percentual")
                        .HasPrecision(5, 2)
                        .HasColumnType("decimal(5,2)");
                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("varchar(12)");
                    b.HasKey("Id");
                    b.HasIndex("CestaId");
                    b.ToTable("ItensCesta", (string)null);
                });
            modelBuilder.Entity("Admin.Service.Infrastructure.Persistence.ItemCestaEntity", b =>
                {
                    b.HasOne("Admin.Service.Infrastructure.Persistence.CestaEntity", "Cesta")
                        .WithMany("Itens")
                        .HasForeignKey("CestaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Cesta");
                });
            modelBuilder.Entity("Admin.Service.Infrastructure.Persistence.CestaEntity", b =>
                {
                    b.Navigation("Itens");
                });
#pragma warning restore 612, 618
        }
    }
}
