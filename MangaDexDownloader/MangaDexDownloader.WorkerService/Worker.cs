using MangaDexDownloader.Shared.Interfaces;
using MangaDexDownloader.Shared.Models.MangaDexApi;

namespace MangaDexDownloader.WorkerService;

public class Worker(
    ILogger<Worker> logger,
    IMangaDexApi mangaDexApi) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IMangaDexApi _mangaDexApi = mangaDexApi;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Search after manga.
        Console.Write("> ");
        string userInput = Console.ReadLine() ?? String.Empty;

        IAsyncEnumerable<MangaSearchDto> searchResults = _mangaDexApi.SearchAfterMangaAsync(userInput);
        List<MangaSearchDto> mangaList = [];

        int counter = 1;

        await foreach (var searchResult in searchResults)
        {
            mangaList.Add(searchResult);
            Console.WriteLine($"{counter} : {searchResult.Name}");
            counter++;
        }

        Console.Write("\n> ");
        string userMangaSelection = Console.ReadLine() ?? String.Empty;
        int.TryParse(userMangaSelection, out var mangaSelection);

        // Get all chapters.
        MangaSearchDto selectedManga = mangaList[mangaSelection - 1];
        IAsyncEnumerable<MangaChapterDto> mangaChapters = _mangaDexApi.GetChapters(selectedManga.Id);
        await foreach (var chapter in mangaChapters)
        {
            Console.WriteLine($"Volume: {chapter.Volume} - Chapter: {chapter.Chapter}");
        }
    }
}
