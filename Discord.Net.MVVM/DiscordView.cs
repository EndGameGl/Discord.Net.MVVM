using System;
using System.Threading.Tasks;
using Discord.Net.MVVM.Services;
using Discord.Net.MVVM.View;
using Discord.Net.MVVM.View.Reactions;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord.Net.MVVM
{
    /// <summary>
    ///     Class that handles discord events and passes them down to it's <see cref="DiscordViewModel" />
    /// </summary>
    public sealed class DiscordView : IAsyncDisposable
    {
        private readonly DiscordMVVMMappingService _mappingService;

        internal DiscordView(
            DiscordViewModel viewModel,
            DiscordEventBindings eventBindings,
            DiscordMVVMMappingService service)
        {
            ViewModel = viewModel;
            HandledEvents = eventBindings;
            _mappingService = service;
        }

        public RestUserMessage Message { get; private set; }
        private DiscordViewBody Body { get; } = new();
        private DiscordViewModel ViewModel { get; }

        public DiscordEventBindings HandledEvents { get; }

        public async ValueTask DisposeAsync()
        {
            await ViewModel.DisposeAsync();
        }

        public async Task Render(IMessageChannel channel)
        {
            ViewModel.InjectData(Body);

            await ViewModel.OnCreate();

            Message = (RestUserMessage)await channel.SendMessageAsync(
                Body.Content.Content,
                false,
                Body.Embed.Embed,
                component: Body.Components.BuildComponent());
        }

        private async Task Update()
        {
            await Message.ModifyAsync(x =>
            {
                if (Body.Content.UpdateNeeded)
                {
                    x.Content = Body.Content.Content;
                    Body.Content.SetUpdateNeeded(false);
                }

                if (Body.Embed.UpdateNeeded)
                {
                    x.Embed = Body.Embed.Embed;
                    Body.Embed.SetUpdateNeeded(false);
                }

                if (Body.Components.UpdateNeeded)
                {
                    x.Components = Body.Components.BuildComponent();
                    Body.Components.SetUpdateNeeded(false);
                }
            });

            if (Body.Reactions.UpdateNeeded)
            {
                if (Body.Reactions.ReactionRequests.Count > 1)
                {
                    while (Body.Reactions.ReactionRequests.TryDequeue(out var reactionRequest))
                        await HandleReactionRequest(reactionRequest);
                }
                else
                {
                    var req = Body.Reactions.ReactionRequests.Dequeue();
                    await HandleReactionRequest(req);
                }
            }
        }

        public async Task ReactionAdded(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction)
        {
            await ViewModel.HandleReactionAdded(reaction);
            await AfterEventHandled();
        }

        public async Task ReactionRemoved(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction)
        {
            await ViewModel.HandleReactionRemoved(reaction);
            await AfterEventHandled();
        }

        public async Task ReactionsCleared(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel)
        {
            await ViewModel.HandleReactionsCleared();
            await AfterEventHandled();
        }

        public async Task ReactionsRemovedForEmote(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            IEmote emote)
        {
            await ViewModel.HandleReactionsRemovedForEmote(emote);
            await AfterEventHandled();
        }

        public async Task OnMessageCommandExecuted(SocketMessageCommand socketMessageCommand)
        {
            await ViewModel.HandleMessageCommandExecuted(socketMessageCommand);
            await AfterEventHandled();
        }
        
        public async Task OnSelectMenuExecuted(SocketMessageComponent interaction)
        {
            if (Body.Components.ButtonMappings.TryGetValue(interaction.Data.CustomId, out var selectMenu))
                await selectMenu.FireEvent(interaction, interaction.Data.Values);
            await AfterEventHandled();
        }

        public async Task OnButtonExecuted(SocketMessageComponent interaction)
        {
            if (Body.Components.ButtonMappings.TryGetValue(interaction.Data.CustomId, out var button))
                await button.FireEvent(interaction);
            await AfterEventHandled();
        }

        private async Task HandleReactionRequest(DiscordReactionRequest request)
        {
            switch (request.Type)
            {
                case DiscordReactionRequestType.Add:
                    await Message.AddReactionAsync(request.Reaction);
                    break;
                case DiscordReactionRequestType.RemoveSelf:
                    await Message.RemoveReactionAsync(request.Reaction, DiscordMVVMMappingService.BotId);
                    break;
                case DiscordReactionRequestType.RemoveAllOfType:
                    await Message.RemoveAllReactionsForEmoteAsync(request.Reaction);
                    break;
                case DiscordReactionRequestType.RemoveAll:
                    await Message.RemoveAllReactionsAsync();
                    break;
            }
        }

        private async Task AfterEventHandled()
        {
            if (Body.HasAnyUpdates)
                await Update();
            if (ViewModel.IsDisposalRequested)
            {
                await ViewModel.DisposeAsync();
                if (ViewModel.DeleteMessageAfterDisposal) await Message.DeleteAsync();

                ulong? guildId = null;
                if (Message.Channel is IGuildChannel guildChannel) guildId = guildChannel.GuildId;

                await _mappingService.RemoveView(Message.Channel.Id, Message.Id, guildId);
            }
        }
    }
}