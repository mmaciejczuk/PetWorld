using Microsoft.EntityFrameworkCore;
using PetWorld.Infrastructure.Persistence.Entities;

namespace PetWorld.Infrastructure.Persistence;

public sealed class PetWorldDbContext : DbContext
{
    public PetWorldDbContext(DbContextOptions<PetWorldDbContext> options) : base(options) { }

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChatMessage>(b =>
        {
            b.ToTable("ChatMessages");
            b.HasKey(x => x.Id);

            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.Question).IsRequired().HasMaxLength(4000);
            b.Property(x => x.Answer).IsRequired().HasMaxLength(4000);
            b.Property(x => x.Iterations).IsRequired();
        });
    }
}
