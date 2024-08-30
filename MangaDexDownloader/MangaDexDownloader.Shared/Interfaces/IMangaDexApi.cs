using MangaDexDownloader.Shared.Models.MangaDexApi;

namespace MangaDexDownloader.Shared.Interfaces;

public interface IMangaDexApi
{
    IAsyncEnumerable<MangaSearchDto> SearchAfterMangaAsync(string title);
    IAsyncEnumerable<MangaChapterDto> GetChapters(string mangaId);
}
