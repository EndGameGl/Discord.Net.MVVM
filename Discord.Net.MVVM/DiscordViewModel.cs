using System;
using System.Threading.Tasks;
using Discord.Net.MVVM.View;
using Discord.WebSocket;

namespace Discord.Net.MVVM
{
    public abstract class DiscordViewModel : IAsyncDisposable
    {
        internal bool IsDisposalRequested { get; private set; }
        internal bool DeleteMessageAfterDisposal { get; private set; } = true;
        protected DiscordViewBody ViewBody { get; private set; }
        public abstract DiscordEventBindings HandledEvents { get; }


        public virtual async ValueTask DisposeAsync()
        {
        }

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

        public virtual async Task HandleMessageCommandExecuted(SocketMessageCommand socketMessageCommand)
        {
        }

        internal void InjectData(DiscordViewBody viewBody)
        {
            ViewBody = viewBody;
        }

        protected async Task RequestDisposal(bool deleteMessage)
        {
            IsDisposalRequested = true;
            DeleteMessageAfterDisposal = deleteMessage;
        }
    }
}