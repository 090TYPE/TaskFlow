using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardColumn> Columns => Set<BoardColumn>();
    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.DisplayName).HasMaxLength(128);
        });

        b.Entity<Board>(e =>
        {
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.HasOne(x => x.Owner)
                .WithMany(u => u.Boards)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<BoardColumn>(e =>
        {
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.HasOne(x => x.Board)
                .WithMany(x => x.Columns)
                .HasForeignKey(x => x.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Card>(e =>
        {
            e.Property(x => x.Title).IsRequired().HasMaxLength(300);
            e.HasOne(x => x.Column)
                .WithMany(x => x.Cards)
                .HasForeignKey(x => x.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
