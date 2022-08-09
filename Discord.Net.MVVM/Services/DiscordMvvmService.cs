using Discord.Net.MVVM.Models;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Net.MVVM.Services
{
    public class DiscordMvvmService
    {
        private readonly ConcurrentDictionary<ulong, ChannelViewContainer> _channelViewContainers;

        private readonly BaseSocketClient _discordClient;

        public DiscordMvvmService(
            BaseSocketClient client)
        {
            _channelViewContainers = new ConcurrentDictionary<ulong, ChannelViewContainer>();
            _discordClient = client;

            switch (_discordClient)
            {
                case DiscordSocketClient discordSocketClient:
                    discordSocketClient.Ready += () =>
                    {
                        BotId = discordSocketClient.CurrentUser.Id;
                        return Task.CompletedTask;
                    };
                    break;
                case DiscordShardedClient discordShardedClient:
                    discordShardedClient.ShardReady += (socketClientShard) =>
                    {
                        BotId = socketClientShard.CurrentUser.Id;
                        return Task.CompletedTask;
                    };
                    break;
            }

            client.MessageDeleted += DiscordClientOnMessageDeleted;
            client.ChannelDestroyed += DiscordClientOnChannelDestroyed;
            client.LeftGuild += DiscordClientOnLeftGuild;

            client.ButtonExecuted += DiscordClientOnButtonExecuted;
            client.SelectMenuExecuted += ClientOnSelectMenuExecuted;
        }

       

        private async Task DiscordClientOnButtonExecuted(
            SocketMessageComponent buttonExecutedEvent)
        {
            if (TryGetView(
                    buttonExecutedEvent.Channel.Id,
                    buttonExecutedEvent.Message.Id, out var view))
            {
                if (view!.SharedData.HandledEvents.Contains(DiscordEventBindings.ButtonExecuted))
                {
                    await view.HandleButtonExecuted(buttonExecutedEvent);
                }
            }
        }
        
        private async Task ClientOnSelectMenuExecuted(
            SocketMessageComponent selectMenuEvent)
        {
            if (TryGetView(
                    selectMenuEvent.Channel.Id,
                    selectMenuEvent.Message.Id, out var view))
            {
                if (view!.SharedData.HandledEvents.Contains(DiscordEventBindings.ButtonExecuted))
                {
                    await view.HandleSelectMenuExecuted(selectMenuEvent);
                }
            }
        }

        private async Task DiscordClientOnLeftGuild(
            SocketGuild guild)
        {
            foreach (var channel in guild.Channels)
            {
                if (_channelViewContainers.TryGetValue(channel.Id, out var channelViewContainer))
                {
                    foreach (var (_, discordView) in channelViewContainer.Views)
                    {
                        if (discordView.ViewModel.DisposeOnMessageDeletion)
                        {
                            await discordView.DisposeAsync();
                        }
                    }
                }

                _channelViewContainers.Remove(channel.Id, out _);
            }
        }

        private async Task DiscordClientOnMessageDeleted(
            Cacheable<IMessage, ulong> cachedMessage,
            Cacheable<IMessageChannel, ulong> cachedChannel)
        {
            if (TryGetView(cachedChannel.Id, cachedMessage.Id, out var view))
            {
                if (view!.ViewModel.DisposeOnMessageDeletion)
                {
                    await view.DisposeAsync();
                }

                _channelViewContainers[cachedChannel.Id].Views.Remove(cachedMessage.Id, out _);
            }
        }

        private async Task DiscordClientOnChannelDestroyed(
            SocketChannel channel)
        {
            if (_channelViewContainers.TryGetValue(channel.Id, out var channelViewContainer))
            {
                foreach (var (_, discordView) in channelViewContainer.Views)
                {
                    if (discordView.ViewModel.DisposeOnMessageDeletion)
                    {
                        await discordView.DisposeAsync();
                    }
                }
            }

            _channelViewContainers.Remove(channel.Id, out _);
        }

        internal static ulong BotId { get; private set; }

        public async Task RespondWithViewAsync(
            DiscordViewModel viewModel,
            IInteractionContext context)
        {
            var view = new DiscordView(viewModel, this);

            await view.Render(context.Interaction);

            AddNewView(view);
        }

        public async Task RemoveView(ulong channelId, ulong messageId)
        {
            if (TryGetView(channelId, messageId, out var view))
            {
                if (view!.ViewModel.DisposeOnMessageDeletion)
                {
                    await view.DisposeAsync();
                }
            }
        }

        private void AddNewView(DiscordView discordView)
        {
            var container = GetOrCreateChannelViewContainer(discordView.SharedData.Message.Channel);
            container.AddView(discordView);
        }

        private ChannelViewContainer GetOrCreateChannelViewContainer(IMessageChannel channel)
        {
            return _channelViewContainers.GetOrAdd(
                channel.Id,
                static (_, channelParam) => new ChannelViewContainer(channelParam), channel);
        }

        private bool TryGetView(ulong channelId, ulong messageId, out DiscordView? view)
        {
            view = null;

            return _channelViewContainers.TryGetValue(channelId, out var container) &&
                   container.TryGetView(messageId, out view);
        }
    }
}