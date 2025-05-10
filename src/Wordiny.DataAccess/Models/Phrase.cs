namespace Wordiny.DataAccess.Models;

public class Phrase
{
    public long Id { get; protected set; }
    public long UserId { get; protected set; }
    public string NativeText { get; protected set; } = string.Empty;
    public string TranslationText { get; protected set; } = string.Empty;
    public MemoryState MemoryState { get; protected set; }

    public Phrase(long userId, string nativeText, string translationText)
    {
        UserId = userId;

        ArgumentException.ThrowIfNullOrWhiteSpace(nativeText);
        NativeText = nativeText;

        ArgumentException.ThrowIfNullOrWhiteSpace(translationText);
        TranslationText = translationText;
    }
}

public enum MemoryState : byte
{
    Learning = 1,
    Repeating = 2,
    Learned = 3
}