using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FLS.CodeBeispiel.KatalogService.Models;

namespace FLS.CodeBeispiel.KatalogService.Configurations;

internal class KundenRegionEntityTypConfiguration : IEntityTypeConfiguration<KundenRegionen>
{
    public void Configure(EntityTypeBuilder<KundenRegionen> entity)
    {
        entity.HasNoKey();

        entity.ToView("vKundenregionen", "CRM");

        entity.Property(e => e.AccountId).HasColumnName("accountid");
    }
}
