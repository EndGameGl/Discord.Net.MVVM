using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Discord.Net.MVVM.View.Controls
{
    public class DiscordActionRow : DiscordControl
    {
        public override DiscordControlType Type => DiscordControlType.ActionRow;

        public override IMessageComponent ToComponent()
        {
            var builder = new ActionRowBuilder();

            foreach (var control in Controls)
            {
                builder.WithComponent(control.ToComponent());
            }
            
            return builder.Build();
        }

        internal override async Task FireEvent(SocketMessageComponent interactionComponent)
        {
            throw new System.NotImplementedException();
        }

        internal override async Task FireEvent(SocketMessageComponent interactionComponent, IReadOnlyCollection<string> values)
        {
            throw new System.NotImplementedException();
        }

        public ActionRowBuilder GetBuilder()
        {
            var builder = new ActionRowBuilder();

            foreach (var control in Controls)
            {
                builder.WithComponent(control.ToComponent());
            }

            return builder;
        }

        public List<DiscordControl> Controls = new();

        public bool IsBuildable()
        {
            if (Controls.Count == 0)
                return false;

            if (Controls.All(x => x.IsControlActive != true))
                return false;

            var activeSelectsCount = Controls.Count(x => x.Type == DiscordControlType.SelectMenu && x.IsControlActive);
            
            var activeButtonsCount = Controls.Count(x => x.Type == DiscordControlType.Button && x.IsControlActive);

            if (activeButtonsCount > 0 && activeSelectsCount > 0)
                return false;

            if (activeSelectsCount > 1)
                return false;

            if (activeButtonsCount > 5)
                return false;
            
            return true;
        }
    }
}