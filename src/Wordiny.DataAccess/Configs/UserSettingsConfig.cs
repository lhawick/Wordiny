using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordiny.DataAccess.Models;

namespace Wordiny.DataAccess.Configs;

internal class UserSettingsConfig : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings", "public");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.RepeatFrequencyInDay).HasColumnName("frequency_in_day");

        builder.HasIndex(x => x.RepeatFrequencyInDay);

        builder
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<UserSettings>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
