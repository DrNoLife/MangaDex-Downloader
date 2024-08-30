namespace MangaDexDownloader.Shared.Models.MangaDexApi;

public record MangaSearchDto(
    string Id,
    string Name,
    string Description,
    IEnumerable<string> Tags);
