namespace Discord.Net.MVVM.View.Controls
{
    public class DiscordSelectMenuOption
    {
        public bool IsDefault { get; set; } = false;
        public string Value { get; set; }
        public string Description { get; set; }
        public IEmote Emote { get; set; }
        public string Label { get; set; }

        public DiscordSelectMenuOption(string label, string value)
        {
            Label = label;
            Value = value;
        }
        
        public SelectMenuOptionBuilder GetBuilder()
        {
            var selectMenuOptionBuilder = new SelectMenuOptionBuilder
            {
                Default = IsDefault,
                Value = Value,
                Description = Description,
                Emote = Emote,
                Label = Label
            };

            return selectMenuOptionBuilder;
        }
    }
}