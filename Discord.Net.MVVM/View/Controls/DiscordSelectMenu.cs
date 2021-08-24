using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Discord.Net.MVVM.View.Controls
{
    public class DiscordSelectMenu : DiscordControl
    {
        public List<DiscordSelectMenuOption> Options { get; } = new();
        public string Placeholder { get; set; }
        public int MinSelectableValues { get; set; } = 1;
        public int MaxSelectableValues { get; set; } = 1;

        public override DiscordControlType Type => DiscordControlType.SelectMenu;

        public override IMessageComponent ToComponent()
        {
            var selectMenuBuilder = new SelectMenuBuilder
            {
                Label = Label,
                CustomId = Id,
                MinValues = MinSelectableValues,
                MaxValues = MaxSelectableValues,
                Placeholder = Placeholder
            };

            if (Options.Count > 0)
            {
                selectMenuBuilder.Options = new List<SelectMenuOptionBuilder>();
            }

            foreach (var option in Options)
            {
                selectMenuBuilder.Options.Add(option.GetBuilder());
            }

            return selectMenuBuilder.Build();
        }

        public event Func<SocketMessageComponent, IReadOnlyCollection<string>, Task> OnSelect;

        internal override async Task FireEvent(SocketMessageComponent interactionComponent)
        {
            throw new System.NotImplementedException();
        }

        internal override async Task FireEvent(SocketMessageComponent interactionComponent, IReadOnlyCollection<string> values)
        {
            if (OnSelect is not null)
            {
                await OnSelect.Invoke(interactionComponent, values);
            }
        }
    }
}