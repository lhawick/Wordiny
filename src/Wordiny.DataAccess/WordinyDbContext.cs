using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Wordiny.DataAccess.Models;

namespace Wordiny.DataAccess;

public class WordinyDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Phrase> Phrases { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    public WordinyDbContext(DbContextOptions<WordinyDbContext> options) : base(options)
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
