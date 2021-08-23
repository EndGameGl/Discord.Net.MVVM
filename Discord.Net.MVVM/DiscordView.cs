using System;
using System.Threading.Tasks;
using Discord.Net.MVVM.Services;
using Discord.Net.MVVM.View;
using Discord.Net.MVVM.View.Controls;
using Discord.Net.MVVM.View.Reactions;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord.Net.MVVM
{
    /// <summary>
    /// Class that handles discord events and passes them down to it's <see cref="DiscordViewModel"/>
    /// </summary>
    public sealed class DiscordView : IAsyncDisposable
    {
        public RestUserMessage Message { get; private set; }
        private DiscordViewBody Body { get; } = new();
        private DiscordViewModel ViewModel { get; }

        public DiscordEventBindings HandledEvents { get; }

        public async Task Render(IMessageChannel channel)
        {
            ViewModel.InjectBody(Body);
            
            await ViewModel.OnCreate();

            Message = (RestUserMessage)await channel.SendMessageAsync(
                text: Body.Content.Content,
                false,
                embed: Body.Embed.Embed,
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
                    {
                        await HandleReactionRequest(reactionRequest);
                    }
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
            if (Body.HasAnyUpdates)
                await Update();
        }

        public async Task ReactionRemoved(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction)
        {
            await ViewModel.HandleReactionRemoved(reaction);
            if (Body.HasAnyUpdates)
                await Update();
        }

        public async Task ReactionsCleared(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel)
        {
            await ViewModel.HandleReactionsCleared();
            if (Body.HasAnyUpdates)
                await Update();
        }

        public async Task ReactionsRemovedForEmote(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            IEmote emote)
        {
            await ViewModel.HandleReactionsRemovedForEmote(emote);
            if (Body.HasAnyUpdates)
                await Update();
        }

        public async Task InteractionCreated(
            SocketMessageComponent interaction)
        {
            switch (interaction.Data.Type)
            {
                case ComponentType.Button:
                    if (Body.Components.ButtonMappings.TryGetValue(interaction.Data.CustomId, out var button))
                    {
                        await button.FireEvent();
                    }

                    break;
                case ComponentType.SelectMenu:
                    if (Body.Components.ButtonMappings.TryGetValue(interaction.Data.CustomId, out var selectMenu))
                    {
                        await selectMenu.FireEvent(interaction.Data.Values);
                    }
                    break;
            }

            await ViewModel.HandleInteractionCreated(interaction);
            if (Body.HasAnyUpdates)
                await Update();
        }

        internal DiscordView(DiscordViewModel viewModel, DiscordEventBindings eventBindings)
        {
            ViewModel = viewModel;
            HandledEvents = eventBindings;
        }

        public async ValueTask DisposeAsync()
        {
            await ViewModel.DisposeAsync();
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
    }
}