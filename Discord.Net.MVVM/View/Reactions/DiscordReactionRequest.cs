namespace Discord.Net.MVVM.View.Reactions
{
    public class DiscordReactionRequest
    {
        public IEmote Reaction { get; init; } 
        public DiscordReactionRequestType Type { get; init; }
    }
}