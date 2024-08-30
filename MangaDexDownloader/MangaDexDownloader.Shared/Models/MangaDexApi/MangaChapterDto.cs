namespace MangaDexDownloader.Shared.Models.MangaDexApi;

public record MangaChapterDto(
    string Id,
    string Title,
    double Volume,
    double Chapter);
