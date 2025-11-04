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
        builder.Property(x => x.ChatId).HasColumnName("chat_id");
        builder.Property(x => x.Username).HasColumnName("username");
        builder.Property(x => x.IsDisabled).HasColumnName("is_disabled");
        builder.Property(x => x.Created).HasColumnName("created");

        builder.HasIndex(x => x.ChatId);
        builder.HasIndex(x => x.IsDisabled);
    }
}
