namespace Discord.Net.MVVM.View.Controls
{
    public class DiscordSelectMenuOption
    {
        public DiscordSelectMenuOption(string label, string value)
        {
            Label = label;
            Value = value;
        }

        public bool IsDefault { get; set; } = false;
        public string Value { get; set; }
        public string Description { get; set; }
        public IEmote Emote { get; set; }
        public string Label { get; set; }

        public SelectMenuOptionBuilder GetBuilder()
        {
            var selectMenuOptionBuilder = new SelectMenuOptionBuilder
            {
                IsDefault = IsDefault,
                Value = Value,
                Description = Description,
                Emote = Emote,
                Label = Label
            };

            return selectMenuOptionBuilder;
        }
    }
}