using Discord.Net.MVVM.View.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Net.MVVM.Examples
{
    public partial class CounterViewModel : DiscordViewModel
    {
        private int _count;
        private bool _isVocal;

        public override bool DisposeOnMessageDeletion => true;

        public override async Task OnCreate()
        {
            ModifyText($"Count is {_count}");
            AddButton(IncreaseCountButton, 0);
            AddButton(DescreaseCountButton, 0);
            AddButton(ChangeButtonNaming, 1);
            AddButton(EnableSelectMenuButton, 2);
            AddSelectMenu(ActionSelectMenu, 3);

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
                if (_isVocal)
                {
                    IncreaseCountButton.Label = "+1";
                    DescreaseCountButton.Label = "-1";
                    _isVocal = false;
                }
                else
                {
                    IncreaseCountButton.Label = "Increment";
                    DescreaseCountButton.Label = "Decrement";
                    _isVocal = true;
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

        public override ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
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