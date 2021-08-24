namespace Discord.Net.MVVM.View
{
    public class DiscordTrackableContent : IDiscordMessageTrackablePart
    {
        public string Content { get; private set; }
        public bool HasValue => !string.IsNullOrWhiteSpace(Content);
        public bool UpdateNeeded { get; private set; }

        public void SetUpdateNeeded(bool value)
        {
            UpdateNeeded = value;
        }

        public void ResetContent()
        {
            Modify(null);
        }

        public void Modify(string value)
        {
            if (Content == value)
                return;
            Content = value;
            SetUpdateNeeded(true);
        }
    }
}