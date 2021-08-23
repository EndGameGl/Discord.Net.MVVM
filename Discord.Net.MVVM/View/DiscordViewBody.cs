namespace Discord.Net.MVVM.View
{
    /// <summary>
    /// Class that represents view body.
    /// </summary>
    public class DiscordViewBody
    {
        /// <summary>
        /// Component that represents message text
        /// </summary>
        public DiscordTrackableContent Content { get; } = new();

        /// <summary>
        /// Component that represents message embed
        /// </summary>
        public DiscordTrackableEmbed Embed { get; } = new();

        /// <summary>
        /// Component that represents message buttons and dropdowns
        /// </summary>
        public DiscordTrackableComponent Components { get; } = new();

        /// <summary>
        /// Component that represents message reactions
        /// </summary>
        public DiscordTrackableReactions Reactions { get; } = new();

        /// <summary>
        /// Checks whether any of the components were updated
        /// </summary>
        public bool HasAnyUpdates => Content.UpdateNeeded ||
                                     Embed.UpdateNeeded ||
                                     Reactions.UpdateNeeded ||
                                     Components.UpdateNeeded;
    }
}