namespace Discord.Net.MVVM.View
{
    public interface IDiscordMessageTrackablePart
    {
        bool HasValue { get; }
        bool UpdateNeeded { get; }
        void SetUpdateNeeded(bool value);
        void ResetContent();
    }
}