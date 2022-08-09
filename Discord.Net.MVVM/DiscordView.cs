using Discord.Net.MVVM.Models;
using Discord.Net.MVVM.Services;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Net.MVVM.View;

namespace Discord.Net.MVVM
{
    /// <summary>
    ///     Class that handles discord events and passes them down to it's <see cref="DiscordViewModel" />
    /// </summary>
    public sealed class DiscordView : IAsyncDisposable
    {
        internal DiscordViewSharedData SharedData { get; }

        internal DiscordView(
            DiscordViewModel viewModel,
            DiscordMvvmService service)
        {
            SharedData = new DiscordViewSharedData()
            {
                DiscordMvvmService = service,
                ViewBody = new DiscordViewBody()
            };

            ViewModel = viewModel;
        }

        internal DiscordViewModel ViewModel { get; }

        public async ValueTask DisposeAsync()
        {
            await SharedData.DisposeAsync();
            await ViewModel.DisposeAsync();
        }

        public async Task Render(IDiscordInteraction socketInteraction)
        {
            ViewModel.InjectInternalData(SharedData);

            await ViewModel.OnCreate();

            await socketInteraction.RespondAsync(
                text: SharedData.ViewBody.Content.Content,
                embed: SharedData.ViewBody.Embed.Embed,
                components: SharedData.ViewBody.Components.BuildComponent());

            SharedData.Message = await socketInteraction.GetOriginalResponseAsync();
        }

        private async Task Update(SocketInteraction interaction)
        {
            await interaction.DeferAsync();
            await interaction.ModifyOriginalResponseAsync(x =>
            {
                if (SharedData.ViewBody.Content.UpdateNeeded)
                {
                    x.Content = SharedData.ViewBody.Content.Content;
                    SharedData.ViewBody.Content.SetUpdateNeeded(false);
                }

                if (SharedData.ViewBody.Embed.UpdateNeeded)
                {
                    x.Embed = SharedData.ViewBody.Embed.Embed;
                    SharedData.ViewBody.Embed.SetUpdateNeeded(false);
                }

                if (SharedData.ViewBody.Components.UpdateNeeded)
                {
                    x.Components = SharedData.ViewBody.Components.BuildComponent();
                    SharedData.ViewBody.Components.SetUpdateNeeded(false);
                }
            });
        }

        private async Task HandleDisposes()
        {
            await SharedData.DisposeAsync();
            await ViewModel.DisposeAsync();
            await SharedData.DiscordMvvmService.RemoveView(
                SharedData.ChannelId,
                SharedData.MessageId);
        }

        internal async Task HandleButtonExecuted(
            SocketMessageComponent buttonExecutedEvent)
        {
            if (SharedData.ViewBody.Components.ButtonMappings.TryGetValue(
                    buttonExecutedEvent.Data.CustomId,
                    out var button))
            {
                if (button.IsControlActive)
                {
                    await button.FireEvent(buttonExecutedEvent);
                    await Update(buttonExecutedEvent);
                }
            }
        }

        internal async Task HandleSelectMenuExecuted(
            SocketMessageComponent selectMenuEvent)
        {
            if (SharedData.ViewBody.Components.ButtonMappings.TryGetValue(
                    selectMenuEvent.Data.CustomId,
                    out var button))
            {
                if (button.IsControlActive)
                {
                    await button.FireEvent(selectMenuEvent, selectMenuEvent.Data.Values);
                    await Update(selectMenuEvent);
                }
            }
        }
    }
}