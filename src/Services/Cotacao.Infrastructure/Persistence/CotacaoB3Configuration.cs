using Cotacao.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cotacao.Infrastructure.Persistence;

internal sealed class CotacaoB3Configuration : IEntityTypeConfiguration<CotacaoB3>
{
    public void Configure(EntityTypeBuilder<CotacaoB3> builder)
    {
        builder.ToTable("Cotacoes");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.DataPregao).IsRequired();
        builder.Property(e => e.Ticker).HasMaxLength(10).IsRequired();
        builder.Property(e => e.PrecoAbertura).HasPrecision(18, 4);
        builder.Property(e => e.PrecoFechamento).HasPrecision(18, 4);
        builder.Property(e => e.PrecoMaximo).HasPrecision(18, 4);
        builder.Property(e => e.PrecoMinimo).HasPrecision(18, 4);

        builder.HasIndex(e => new { e.DataPregao, e.Ticker })
            .IsUnique()
            .HasDatabaseName("IX_Cotacoes_DataPregao_Ticker");

        builder.HasIndex(e => e.DataPregao)
            .HasDatabaseName("IX_Cotacoes_DataPregao");
    }
}
