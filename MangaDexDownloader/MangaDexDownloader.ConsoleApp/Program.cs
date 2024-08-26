using System.IO;
using System.Text.Json.Nodes;

// Basic setup.
const string BaseUri = "https://api.mangadex.org";

HttpClient httpClient = new();
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MDD-CSharp-001");

// Use methods to get a bunch of data.
string mangaId = "8ebf69e8-2f80-4f1a-b537-43771049be63";
IEnumerable<string> chapterIds = await GetChapters(mangaId);

Dictionary<string, IEnumerable<Uri>> chaptersWithImageUris = [];

foreach(string chapterId in chapterIds)
{
    if (String.IsNullOrEmpty(chapterId))
    {
        continue;
    }

    var imageUris = await GetChapterImageUris(chapterId);
    chaptersWithImageUris.Add(chapterId, imageUris);
}

// Start downloading chapter images.
await DownloadChapterImages(mangaId, chaptersWithImageUris);

async Task<IEnumerable<string>> GetChapters(string mangaId)
{
    string getChaptersRequestUri = $"{BaseUri}/manga/{mangaId}/feed";
    HttpResponseMessage responseMessage = await httpClient.GetAsync(getChaptersRequestUri);
    string responseJson = await responseMessage.Content.ReadAsStringAsync();

    // Handle it as json.
    JsonNode? response = JsonObject.Parse(responseJson);

    if (response is null)
    {
        Console.WriteLine("Failed to parse json response into a json object.");
        return [];
    }

    var data = response["data"]!.AsArray();

    return data
        .Where(x => x!["attributes"]!["translatedLanguage"]!.GetValue<string>() == "en")
        .Select(x => x?["id"]!.ToString() ?? String.Empty);
}

async Task<IEnumerable<Uri>> GetChapterImageUris(string chapterId, bool fullQuality = true)
{
    string baseUriForChapterFeed = $"{BaseUri}/at-home/server/{chapterId}";

    var responseMessage = await httpClient.GetStringAsync(baseUriForChapterFeed);
    var response = JsonObject.Parse(responseMessage);

    string baseUrl = response!["baseUrl"]!.GetValue<string>();

    var chapterData = response!["chapter"];
    string chapterHash = chapterData!["hash"]!.GetValue<string>();

    string qualitySpecifier = fullQuality ? "data" : "dataSaver";

    return chapterData[qualitySpecifier]!
        .AsArray()
        .Select(x => new Uri($"{baseUrl}/{qualitySpecifier}/{chapterHash}/{x}"));
}

async Task DownloadChapterImages(string mangaId, Dictionary<string, IEnumerable<Uri>> chapterWithImages)
{
    foreach(var chapterWithImageUris in chaptersWithImageUris)
    {
        string mangaDirectory = $"manga/{mangaId}/{chapterWithImageUris.Key}";

        // Make sure the folder exists.
        Directory.CreateDirectory(mangaDirectory);

        await Parallel.ForEachAsync(chapterWithImageUris.Value, async (uri, token) =>
        {
            // Download image data.
            var byteArray = await httpClient.GetByteArrayAsync(uri, token);

            string filename = $"{mangaDirectory}/{Path.GetFileName(uri.ToString())}";

            // Copy byte array to image.
            using MemoryStream memoryStream = new(byteArray);
            using FileStream fileStream = new(filename, FileMode.Create);
            memoryStream.WriteTo(fileStream);
        });
    }
}
