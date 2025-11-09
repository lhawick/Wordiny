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

        builder.HasIndex(x => x.IsDisabled);
    }
}
