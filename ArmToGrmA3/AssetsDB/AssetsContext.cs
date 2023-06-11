using ArmaRealMapWebSite.Entities.Assets;
using Microsoft.EntityFrameworkCore;

namespace ArmaRealMapWebSite.Entities
{
    public class AssetsContext : DbContext
    {
        private readonly string path;

        public AssetsContext(string path)
        {
            this.path = path;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source="+path);
        }

        public DbSet<Asset> Assets { get; set; }

        public DbSet<AssetPreview> AssetPreviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>().ToTable(nameof(Asset));
            modelBuilder.Entity<AssetPreview>().ToTable(nameof(AssetPreview));
        }
    }
}
