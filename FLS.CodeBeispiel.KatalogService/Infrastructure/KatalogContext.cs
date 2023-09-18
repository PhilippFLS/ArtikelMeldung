using Microsoft.EntityFrameworkCore;
using FLS.CodeBeispiel.KatalogService.Configurations;
using FLS.CodeBeispiel.KatalogService.Models;

namespace FLS.CodeBeispiel.KatalogService.Infrastructure;

public class KatalogContext : DbContext
{
    public KatalogContext()
    {
    }

    public KatalogContext(DbContextOptions<KatalogContext> options)
        : base(options)
    {
    }
    public virtual DbSet<KundenRegionen> vKundenregionen { get; set; }
    public virtual DbSet<Region> vKatalogRegion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AI");

        new KundenRegionEntityTypConfiguration().Configure(modelBuilder.Entity<KundenRegionen>());
        new RegionEntityTypeConfiguration().Configure(modelBuilder.Entity<Region>());

        base.OnModelCreating(modelBuilder);
    }
}