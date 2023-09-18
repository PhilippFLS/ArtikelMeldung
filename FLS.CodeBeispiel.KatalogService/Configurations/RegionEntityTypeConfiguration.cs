using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FLS.CodeBeispiel.KatalogService.Models;

namespace FLS.CodeBeispiel.KatalogService.Configurations;

internal class RegionEntityTypeConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> entity)
    {
        entity.HasNoKey();

        entity.ToView("vKatalogRegion", "Katalog");
    }
}
