using System.Linq;
using System.Threading.Tasks;
using Discord.Net.MVVM.View.Controls;
using Discord.WebSocket;

namespace Discord.Net.MVVM.Examples
{
    public partial class CounterViewModel : DiscordViewModel
    {
        private int _count;

        private bool isVocal;

        public override DiscordEventBindings HandledEvents { get; } =
            DiscordEventBindings.ReactionAdded |
            DiscordEventBindings.ReactionRemoved |
            DiscordEventBindings.InteractionCreated;

        public override async Task OnCreate()
        {
            ViewBody.Content.Modify($"Count is {_count}");
            ViewBody.Components.AddButton(IncreaseCountButton, 0);
            ViewBody.Components.AddButton(DescreaseCountButton, 1);
            ViewBody.Components.AddButton(ChangeButtonNaming, 2);
            ViewBody.Components.AddButton(EnableSelectMenuButton, 3);
            ViewBody.Components.AddButton(ActionSelectMenu, 4);

            ChangeButtonNaming.Style = ButtonStyle.Secondary;

            IncreaseCountButton.OnClick += async _ =>
            {
                _count++;
                HandleValueChange();
            };

            DescreaseCountButton.OnClick += async _ =>
            {
                _count--;
                HandleValueChange();
            };

            ChangeButtonNaming.OnClick += async _ =>
            {
                if (isVocal)
                {
                    IncreaseCountButton.Label = "+1";
                    DescreaseCountButton.Label = "-1";
                    isVocal = false;
                }
                else
                {
                    IncreaseCountButton.Label = "Increment";
                    DescreaseCountButton.Label = "Decrement";
                    isVocal = true;
                }
            };

            EnableSelectMenuButton.OnClick += async _ =>
            {
                ActionSelectMenu.IsControlActive = !ActionSelectMenu.IsControlActive;
                switch (ActionSelectMenu.IsControlActive)
                {
                    case true:
                        EnableSelectMenuButton.Label = "Disable select menu";
                        break;
                    case false:
                        EnableSelectMenuButton.Label = "Enable select menu";
                        break;
                }
            };

            ActionSelectMenu.OnSelect += async (_, values) =>
            {
                var operation = values.First();
                switch (operation)
                {
                    case "inc":
                        _count++;
                        HandleValueChange();
                        break;
                    case "dec":
                        _count--;
                        HandleValueChange();
                        break;
                }
            };
        }

        public override async Task HandleReactionAdded(SocketReaction reaction)
        {
            _count++;
            HandleValueChange();
            ViewBody.Reactions.AddReaction(reaction.Emote);
        }

        public override async Task HandleReactionRemoved(SocketReaction reaction)
        {
            _count--;
            HandleValueChange();
            ViewBody.Reactions.RemoveReaction(reaction.Emote);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            _count = 0;
        }

        private void HandleValueChange()
        {
            if (_count % 2 == 0)
            {
                ViewBody.Content.Modify($"Count is {_count}");
                ViewBody.Embed.ModifyContent(null);
            }
            else
            {
                ViewBody.Content.Modify(null);
                var eb = new EmbedBuilder
                {
                    Title = "Count is not even",
                    Description = $"Count is {_count}"
                };

                ViewBody.Embed.ModifyContent(eb.Build());
            }
        }
    }

    public partial class CounterViewModel
    {
        private readonly DiscordSelectMenu ActionSelectMenu =
            new()
            {
                IsControlActive = false,
                MinSelectableValues = 1,
                MaxSelectableValues = 1,
                Id = nameof(ActionSelectMenu),
                Placeholder = "Choose operation",
                Options =
                {
                    new DiscordSelectMenuOption("Increment", "inc"),
                    new DiscordSelectMenuOption("Decrement", "dec")
                }
            };

        private readonly DiscordButton ChangeButtonNaming =
            new(nameof(ChangeButtonNaming), "Change naming")
            {
                IsControlActive = true
            };

        private readonly DiscordButton DescreaseCountButton =
            new(nameof(DescreaseCountButton), "-1")
            {
                IsControlActive = true
            };

        private readonly DiscordButton EnableSelectMenuButton =
            new(nameof(EnableSelectMenuButton), "Enable select menu")
            {
                IsControlActive = true
            };

        private readonly DiscordButton IncreaseCountButton =
            new(nameof(IncreaseCountButton), "+1")
            {
                IsControlActive = true
            };
    }
}