using System;

namespace Discord.Net.MVVM
{
    [Flags]
    public enum DiscordEventBindings
    {
        ReactionAdded = 1 << 0,
        ReactionRemoved = 1 << 1,
        ReactionsCleared = 1 << 2,
        ReactionsRemovedForEmote = 1 << 3,
        InteractionCreated = 1 << 4
    }
}