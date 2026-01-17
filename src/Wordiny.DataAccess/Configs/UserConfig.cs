using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordiny.DataAccess.Models;

namespace Wordiny.DataAccess.Configs;

internal class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "public");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled");
        builder.Property(x => x.Created).HasColumnName("created");
        builder.Property(x => x.Updated).HasColumnName("updated");
        builder.Property(x => x.InputState).HasColumnName("input_state");

        builder.HasIndex(x => x.IsDisabled);

        builder
            .HasMany<Phrase>()
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<UserSettings>()
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
