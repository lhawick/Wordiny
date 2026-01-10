namespace Wordiny.DataAccess.Models;

public class Phrase
{
    public long Id { get; protected set; }
    public long UserId { get; protected set; }
    public string NativeText { get; protected set; } = string.Empty;
    public string? TranslationText { get; protected set; }
    public MemoryState MemoryState { get; protected set; }
    public DateTimeOffset Added { get; protected set; }
    public long PhraseTgMessageId { get; protected set; }
    public long TranslationTgMessageId { get; protected set; }

    public Phrase(long userId, string nativeText, string? translationText = null)
    {
        UserId = userId;

        ArgumentException.ThrowIfNullOrWhiteSpace(nativeText);
        NativeText = nativeText;

        TranslationText = translationText;
        Added = DateTimeOffset.UtcNow;
    }

    public void AddTranslation(string translation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(translation, nameof(translation));
        TranslationText = translation;

        MemoryState = MemoryState.Learning;
    }
}

public enum MemoryState : byte
{
    NoShowed = 0,
    Learning = 1,
    Repeating = 2,
    Learned = 3
}