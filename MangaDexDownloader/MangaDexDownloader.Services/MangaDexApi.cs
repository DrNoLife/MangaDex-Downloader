using MangaDexDownloader.Shared.Interfaces;
using MangaDexDownloader.Shared.Models.MangaDexApi;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace MangaDexDownloader.Services;

public class MangaDexApi(
    IHttpClientFactory httpClientFactory,
    ILogger<MangaDexApi> logger) 
    : IMangaDexApi
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<MangaDexApi> _logger = logger;

    public async IAsyncEnumerable<MangaSearchDto> SearchAfterMangaAsync(string title)
    {
        var httpClient = _httpClientFactory.CreateClient("MangaDexApi");
        string responseJson = await httpClient.GetStringAsync($"manga?title={title}");

        JsonNode responseObject = JsonObject.Parse(responseJson) ?? throw new Exception("Failed to parse json from response.");

        var dataArray = responseObject["data"];
        if (dataArray is null)
        {
            yield break; 
        }

        foreach (JsonNode? data in dataArray.AsArray())
        {
            var mangaAttributes = data!["attributes"];
            if (mangaAttributes is null)
            {
                continue;
            }

            string mangaId = data["id"]!.GetValue<string>();
            string? mangaTitle = mangaAttributes["title"]?["en"]?.GetValue<string>();

            if (String.IsNullOrEmpty(mangaTitle))
            {
                continue;
            }

            string mangaDescription = mangaAttributes["description"]?["en"]?.GetValue<string>() ?? String.Empty;

            JsonArray mangaTags = mangaAttributes["tags"]!.AsArray();

            IEnumerable<string> tags = mangaTags
                .Where(tag => tag != null)
                .Select(tag => tag?["attributes"]?["name"]?["en"]?.GetValue<string>())
                .OfType<string>();

            yield return new MangaSearchDto(
                mangaId,
                mangaTitle,
                mangaDescription,
                tags);
        }
    }

    public async IAsyncEnumerable<MangaChapterDto> GetChapters(string mangaId)
    {
        var httpClient = _httpClientFactory.CreateClient("MangaDexApi");
        string responseJson = await httpClient.GetStringAsync($"manga/{mangaId}/feed");

        JsonNode responseObject = JsonObject.Parse(responseJson) ?? throw new Exception("Failed to parse json from response.");

        var dataArray = responseObject["data"];
        if (dataArray is null)
        {
            yield break;
        }

        foreach (var x in dataArray.AsArray())
        {
            if (x?["attributes"]?["translatedLanguage"]?.GetValue<string>() != "en")
            {
                continue;
            }

            string id = x["id"]?.GetValue<string>() ?? String.Empty;

            var attributes = x["attributes"];
            if (attributes is null)
            {
                continue;
            }

            string title = attributes["title"]?.GetValue<string>() ?? String.Empty;
            double volume = Double.Parse(attributes["volume"]?.GetValue<string>() ?? "0");
            double chapter = Double.Parse(attributes["chapter"]?.GetValue<string>() ?? "0");

            var mangaChapterDto = new MangaChapterDto(id, title, volume, chapter);
            yield return mangaChapterDto;
        }

    }
}
