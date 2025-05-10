using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordiny.DataAccess.Models;

namespace Wordiny.DataAccess.Configs;

internal class PhraseConfig : IEntityTypeConfiguration<Phrase>
{
    public void Configure(EntityTypeBuilder<Phrase> builder)
    {
        builder.ToTable("phrases", "public");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.NativeText).HasColumnName("native_text").IsRequired();
        builder.Property(x => x.TranslationText).HasColumnName("translation_text").IsRequired();
        builder.Property(x => x.MemoryState).HasColumnName("memory_state");

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.MemoryState);
    }
}
