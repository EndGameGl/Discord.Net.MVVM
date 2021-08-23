using Discord.Net.MVVM.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Net.MVVM
{
    public static class DiExtensions
    {
        public static IServiceCollection UseDiscordNetMvvm(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<DiscordMVVMMappingService>();
        }
    }
}