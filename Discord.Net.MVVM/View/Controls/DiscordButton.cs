using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Net.MVVM.View.Controls
{
    /// <summary>
    ///     Represents button that is clicked
    /// </summary>
    public class DiscordButton : DiscordControl
    {
        public DiscordButton(string id, string label)
        {
            Id = id;
            Label = label;
        }

        public IEmote Emote { get; set; }
        public ButtonStyle Style { get; set; } = ButtonStyle.Success;
        public string Url { get; set; } = null;
        public string Label { get; set; }

        public override DiscordControlType Type => DiscordControlType.Button;

        public override IMessageComponent ToComponent()
        {
            var bb = new ButtonBuilder
            {
                CustomId = Id,
                IsDisabled = Disabled,
                Label = Label,
                Emote = Emote,
                Style = Style,
                Url = Url
            };
            return bb.Build();
        }

        internal override async Task FireEvent(SocketMessageComponent interactionComponent)
        {
            if (OnClick is not null)
                await OnClick.Invoke(interactionComponent);
        }

        internal override async Task FireEvent(
            SocketMessageComponent interactionComponent,
            IReadOnlyCollection<string> values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Is called when this button is clicked
        /// </summary>
        public event Func<SocketMessageComponent, Task>? OnClick;
    }
}