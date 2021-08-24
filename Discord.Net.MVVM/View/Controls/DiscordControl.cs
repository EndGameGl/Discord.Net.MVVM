using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Discord.Net.MVVM.View.Controls
{
    public abstract class DiscordControl
    {
        public abstract DiscordControlType Type { get; }
        public string Id { get; set; }
        public string Label { get; set; }
        public bool IsControlActive { get; set; }

        public abstract IMessageComponent ToComponent();
        internal abstract Task FireEvent(SocketMessageComponent interactionComponent);

        internal abstract Task FireEvent(SocketMessageComponent interactionComponent,
            IReadOnlyCollection<string> values);
    }
}