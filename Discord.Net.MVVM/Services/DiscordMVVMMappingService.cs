using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Discord.Net.MVVM.Services
{
    public class DiscordMVVMMappingService
    {
        private readonly DiscordSocketClient _discordSocketClient;

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DiscordView>> _dmViewMapping = new();

        private readonly ConcurrentDictionary<ulong,
                ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DiscordView>>>
            _guildViewMapping = new();

        public DiscordMVVMMappingService(
            DiscordSocketClient client)
        {
            _discordSocketClient = client;

            _discordSocketClient.MessageDeleted += DiscordSocketClientOnMessageDeleted;
            _discordSocketClient.MessagesBulkDeleted += DiscordSocketClientOnMessagesBulkDeleted;
            _discordSocketClient.ChannelDestroyed += DiscordSocketClientOnChannelDestroyed;
            _discordSocketClient.LeftGuild += DiscordSocketClientOnLeftGuild;

            _discordSocketClient.ReactionAdded += DiscordSocketClientOnReactionAdded;
            _discordSocketClient.ReactionRemoved += DiscordSocketClientOnReactionRemoved;
            _discordSocketClient.ReactionsCleared += DiscordSocketClientOnReactionsCleared;
            _discordSocketClient.ReactionsRemovedForEmote += DiscordSocketClientOnReactionsRemovedForEmote;

            _discordSocketClient.ButtonExecuted += DiscordSocketClientOnButtonExecuted;
            _discordSocketClient.SelectMenuExecuted += DiscordSocketClientOnSelectMenuExecuted;

            _discordSocketClient.MessageCommandExecuted += DiscordSocketClientOnMessageCommandExecuted;

            _discordSocketClient.Ready += async () => { BotId = _discordSocketClient.CurrentUser.Id; };
        }

        internal static ulong BotId { get; private set; }

        private async Task DiscordSocketClientOnLeftGuild(SocketGuild guild)
        {
            if (_guildViewMapping.TryRemove(guild.Id, out var channelViews))
                foreach (var channel in channelViews)
                foreach (var messageView in channel.Value)
                    await messageView.Value.DisposeAsync();
        }

        private async Task DiscordSocketClientOnChannelDestroyed(SocketChannel channel)
        {
            if (channel is IGuildChannel guildChannel)
            {
                if (_guildViewMapping.TryGetValue(guildChannel.GuildId, out var channelViews))
                    if (channelViews.TryRemove(channel.Id, out var messageViews))
                        foreach (var messageView in messageViews)
                            await messageView.Value.DisposeAsync();
            }
            else
            {
                if (_dmViewMapping.TryRemove(channel.Id, out var messageViews))
                    foreach (var messageView in messageViews)
                        await messageView.Value.DisposeAsync();
            }
        }

        private async Task DiscordSocketClientOnMessagesBulkDeleted(
            IReadOnlyCollection<Cacheable<IMessage, ulong>> messages,
            Cacheable<IMessageChannel, ulong> channel)
        {
            if (channel.HasValue)
            {
                if (channel.Value is IGuildChannel guildChannel)
                {
                    if (_guildViewMapping.TryGetValue(guildChannel.GuildId, out var channelViews))
                        if (channelViews.TryGetValue(channel.Id, out var messageViews))
                            foreach (var deletedMessage in messages)
                                if (messageViews.TryRemove(deletedMessage.Id, out var messageView))
                                    await messageView.DisposeAsync();
                }
                else
                {
                    if (_dmViewMapping.TryGetValue(channel.Id, out var messageViews))
                        foreach (var deletedMessage in messages)
                            if (messageViews.TryRemove(deletedMessage.Id, out var messageView))
                                await messageView.DisposeAsync();
                }
            }
        }

        private async Task DiscordSocketClientOnMessageDeleted(
            Cacheable<IMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel)
        {
            if (channel.HasValue)
            {
                if (channel.Value is IGuildChannel guildChannel)
                {
                    if (_guildViewMapping.TryGetValue(guildChannel.GuildId, out var channelViews))
                        if (channelViews.TryGetValue(channel.Id, out var messageViews))
                            if (messageViews.TryRemove(message.Id, out var messageView))
                                await messageView.DisposeAsync();
                }
                else
                {
                    if (_dmViewMapping.TryGetValue(channel.Id, out var messageViews))
                        if (messageViews.TryRemove(message.Id, out var messageView))
                            await messageView.DisposeAsync();
                }
            }
        }

        private async Task DiscordSocketClientOnReactionsRemovedForEmote(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            IEmote emote)
        {
            if (TryGetDiscordView(message.Id, channel.Id, GetGuildIdOrNull(channel.Value), out var view))
            {
                if (view.HandledEvents.HasFlag(DiscordEventBindings.ReactionsRemovedForEmote))
                    await view.ReactionsRemovedForEmote(message, channel, emote);
            }
        }

        private async Task DiscordSocketClientOnReactionsCleared(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel)
        {
            if (TryGetDiscordView(message.Id, channel.Id, GetGuildIdOrNull(channel.Value), out var view))
            {
                if (view.HandledEvents.HasFlag(DiscordEventBindings.ReactionsCleared))
                    await view.ReactionsCleared(message, channel);
            }
        }

        private async Task DiscordSocketClientOnReactionRemoved(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction)
        {
            if (TryGetDiscordView(message.Id, channel.Id, GetGuildIdOrNull(channel.Value), out var view))
            {
                if (view.HandledEvents.HasFlag(DiscordEventBindings.ReactionRemoved))
                    await view.ReactionRemoved(message, channel, reaction);
            }
        }

        private async Task DiscordSocketClientOnReactionAdded(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction)
        {
            if (TryGetDiscordView(message.Id, channel.Id, GetGuildIdOrNull(channel.Value), out var view))
            {
                if (view.HandledEvents.HasFlag(DiscordEventBindings.ReactionAdded))
                    await view.ReactionAdded(message, channel, reaction);
            }
        }

        private async Task DiscordSocketClientOnMessageCommandExecuted(
            SocketMessageCommand socketMessageCommand)
        {
            if (TryGetDiscordView(
                messageId: socketMessageCommand.Data.Message.Id,
                channelId: socketMessageCommand.Channel.Id,
                guildId: GetGuildIdOrNull(socketMessageCommand.Channel),
                out var view))
            {
                if (view.HandledEvents.HasFlag(DiscordEventBindings.MessageCommandExecuted))
                {
                    await view.OnMessageCommandExecuted(socketMessageCommand);
                }
            }
        }

        private async Task DiscordSocketClientOnSelectMenuExecuted(
            SocketMessageComponent socketMessageComponent)
        {
            if (TryGetDiscordView(
                messageId: socketMessageComponent.Message.Id,
                channelId: socketMessageComponent.Channel.Id,
                guildId: GetGuildIdOrNull(socketMessageComponent.Channel),
                out var view))
            {
                if (view.HandledEvents.HasFlag(DiscordEventBindings.InteractionCreated))
                {
                    await view.OnSelectMenuExecuted(socketMessageComponent);
                }
            }
        }

        private async Task DiscordSocketClientOnButtonExecuted(
            SocketMessageComponent socketMessageComponent)
        {
            if (TryGetDiscordView(
                messageId: socketMessageComponent.Message.Id,
                channelId: socketMessageComponent.Channel.Id,
                guildId: GetGuildIdOrNull(socketMessageComponent.Channel),
                out var view))
            {
                if (view.HandledEvents.HasFlag(DiscordEventBindings.InteractionCreated))
                {
                    await view.OnButtonExecuted(socketMessageComponent);
                }
            }
        }

        private bool TryGetDiscordView(
            ulong messageId,
            ulong channelId,
            ulong? guildId,
            out DiscordView view)
        {
            view = null;
            if (guildId.HasValue)
            {
                return _guildViewMapping.TryGetValue(guildId.Value, out var channelMappings) &&
                       channelMappings.TryGetValue(channelId, out var messageMappings) &&
                       messageMappings.TryGetValue(messageId, out view);
            }
            else
            {
                return _dmViewMapping.TryGetValue(channelId, out var messageMappings) &&
                       messageMappings.TryGetValue(messageId, out view);
            }
        }

        public async Task CreateView(DiscordViewModel viewModel, IMessageChannel channel)
        {
            var view = new DiscordView(viewModel, viewModel.HandledEvents, this);

            await view.Render(channel);

            if (channel is IGuildChannel guildChannel)
            {
                GuildAddStep:
                if (_guildViewMapping.TryGetValue(guildChannel.GuildId, out var channelViews))
                {
                    ChannelAddStep:
                    if (channelViews.TryGetValue(guildChannel.Id, out var messageViews))
                    {
                        messageViews.TryAdd(view.Message.Id, view);
                    }
                    else
                    {
                        channelViews.TryAdd(guildChannel.Id, new ConcurrentDictionary<ulong, DiscordView>());
                        goto ChannelAddStep;
                    }
                }
                else
                {
                    _guildViewMapping.TryAdd(guildChannel.GuildId,
                        new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DiscordView>>());
                    goto GuildAddStep;
                }
            }
            else
            {
                DmChannelAddStep:
                if (_dmViewMapping.TryGetValue(channel.Id, out var messageViews))
                {
                    messageViews.TryAdd(view.Message.Id, view);
                }
                else
                {
                    _dmViewMapping.TryAdd(channel.Id, new ConcurrentDictionary<ulong, DiscordView>());
                    goto DmChannelAddStep;
                }
            }
        }

        public async Task RemoveView(ulong channelId, ulong messageId, ulong? guildId)
        {
            if (guildId.HasValue)
            {
                if (_guildViewMapping.TryGetValue(guildId.Value, out var channelViews))
                    if (channelViews.TryGetValue(channelId, out var messageViews))
                        if (messageViews.TryRemove(messageId, out var messageView))
                            await messageView.DisposeAsync();
            }
            else
            {
                if (_dmViewMapping.TryGetValue(channelId, out var messageViews))
                    if (messageViews.TryRemove(messageId, out var messageView))
                        await messageView.DisposeAsync();
            }
        }

        private ulong? GetGuildIdOrNull(IMessageChannel messageChannel)
        {
            if (messageChannel is IGuildChannel guildChannel)
            {
                return guildChannel.GuildId;
            }

            return null;
        }
    }
}