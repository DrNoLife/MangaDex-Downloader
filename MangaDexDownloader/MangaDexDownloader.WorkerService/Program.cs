using MangaDexDownloader.Services.Extensions;
using MangaDexDownloader.WorkerService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient("MangaDexApi", options =>
{
    options.DefaultRequestHeaders.UserAgent.ParseAdd("MDD-CSharp-001");
    options.BaseAddress = new Uri("https://api.mangadex.org");
});
builder.Services.AddMangaDexServices();

var host = builder.Build();
host.Run();
