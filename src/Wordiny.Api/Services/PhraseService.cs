using Microsoft.EntityFrameworkCore;
using Wordiny.DataAccess;
using Wordiny.DataAccess.Models;

namespace Wordiny.Api.Services;

public interface IPhraseService
{
    Task AddNewPhraseAsync(long userId, string phrase, CancellationToken token = default);
    Task AddPhraseTranslationAsync(long userId, string translation, CancellationToken token = default);
    Task RemovePhraseAsync(long phraseId, CancellationToken token = default);
}

public class PhraseService : IPhraseService
{
    private readonly WordinyDbContext _db;

    public PhraseService(WordinyDbContext db)
    {
        _db = db;
    }

    public async Task AddNewPhraseAsync(long userId, string phrase, CancellationToken token = default)
    {
        var newPhrase = new Phrase(userId, phrase);
        _db.Add(newPhrase);

        await _db.SaveChangesAsync(token);
    }

    public async Task AddPhraseTranslationAsync(long userId, string translation, CancellationToken token = default)
    {
        var lastPhrase = await _db.Phrases
            .OrderBy(x => x.Added)
            .LastOrDefaultAsync(x => x.UserId == userId, token);

        if (lastPhrase is null)
        {
            throw new InvalidOperationException($"Failed to add translation to non existing phrase (userId: {userId})");
        }

        lastPhrase.AddTranslation(translation);

        await _db.SaveChangesAsync(token);
    }

    public async Task RemovePhraseAsync(long phraseId, CancellationToken token = default)
    {
        await _db.Phrases.Where(x => x.Id == phraseId).ExecuteDeleteAsync(token);
    }
}
