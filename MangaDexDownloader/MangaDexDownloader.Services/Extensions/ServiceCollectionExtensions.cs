using MangaDexDownloader.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MangaDexDownloader.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddMangaDexServices(this IServiceCollection services)
    {
        services.AddSingleton<IMangaDexApi, MangaDexApi>();
    }
}
