using System;

namespace Discord.Net.MVVM
{
    [Flags]
    public enum DiscordEventBindings
    {
        ButtonExecuted,
        SelectMenuExecuted
    }
}