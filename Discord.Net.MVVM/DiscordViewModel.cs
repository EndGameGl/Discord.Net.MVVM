using Discord.Net.MVVM.Models;
using Discord.Net.MVVM.View;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Discord.Net.MVVM.View.Controls;

namespace Discord.Net.MVVM
{
    public abstract class DiscordViewModel : IAsyncDisposable
    {
        private DiscordViewSharedData _sharedData;

        protected DiscordViewBody ViewBody => _sharedData.ViewBody;

        public abstract ValueTask DisposeAsync();
        public abstract bool DisposeOnMessageDeletion { get; }

        public abstract Task OnCreate();

        internal void InjectInternalData(DiscordViewSharedData sharedData)
        {
            _sharedData = sharedData;
        }

        protected void AddButton(DiscordButton discordButton, int row = 0)
        {
            AddHandledEvent(DiscordEventBindings.ButtonExecuted);
            ViewBody.Components.AddButton(discordButton, row);
        }

        protected void AddSelectMenu(DiscordSelectMenu selectMenu, int row = 0)
        {
            AddHandledEvent(DiscordEventBindings.SelectMenuExecuted);
            ViewBody.Components.AddButton(selectMenu, row);
        }

        protected void ModifyText(string? text)
        {
            ViewBody.Content.Modify(text);
        }
        
        protected void RequestDisposal(bool deleteMessage, bool disposeBeforeLastUpdate)
        {
            _sharedData.RequestDisposal(deleteMessage, disposeBeforeLastUpdate);
        }

        protected void AddHandledEvent(DiscordEventBindings discordEventBindings)
        {
            _sharedData.HandledEvents.Add(discordEventBindings);
        }

        protected void RemoveHandledEvent(DiscordEventBindings discordEventBindings)
        {
            _sharedData.HandledEvents.Remove(discordEventBindings);
        }
    }
}