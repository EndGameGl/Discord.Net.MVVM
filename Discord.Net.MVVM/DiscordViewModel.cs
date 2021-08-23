using System;
using System.Threading.Tasks;
using Discord.Net.MVVM.View;
using Discord.WebSocket;

namespace Discord.Net.MVVM
{
    public abstract class DiscordViewModel : IAsyncDisposable
    {
        protected DiscordViewBody ViewBody { get; private set; }
        public abstract DiscordEventBindings HandledEvents { get; }
        
        public virtual async Task OnCreate()
        {
        }

        public virtual async Task HandleReactionAdded(SocketReaction reaction)
        {
        }

        public virtual async Task HandleReactionRemoved(SocketReaction reaction)
        {
        }

        public virtual async Task HandleReactionsCleared()
        {
        }

        public virtual async Task HandleReactionsRemovedForEmote(IEmote emote)
        {
        }

        public virtual async Task HandleInteractionCreated(SocketMessageComponent interaction)
        {
        }


        public virtual async ValueTask DisposeAsync()
        {
            
        }

        internal void InjectBody(DiscordViewBody viewBody)
        {
            ViewBody = viewBody;
        }
    }
}