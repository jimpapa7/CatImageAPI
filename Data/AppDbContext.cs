using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<CatEntity> Cats { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    public DbSet<CatTag> CatTags { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define composite key for CatTag
        modelBuilder.Entity<CatTag>()
            .HasKey(ct => new { ct.CatId, ct.TagId }); // Composite primary key

        // Configure foreign key relationships without navigation properties
        modelBuilder.Entity<CatTag>()
            .HasOne<CatEntity>()
            .WithMany()
            .HasForeignKey(ct => ct.CatId)  // Foreign key from CatTag to CatEntity
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CatTag>()
            .HasOne<TagEntity>()
            .WithMany()
            .HasForeignKey(ct => ct.TagId)  // Foreign key from CatTag to TagEntity
            .OnDelete(DeleteBehavior.Cascade);
    }
}
