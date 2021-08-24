using System.Collections.Generic;
using System.Linq;
using Discord.Net.MVVM.View.Reactions;

namespace Discord.Net.MVVM.View
{
    public class DiscordTrackableReactions : IDiscordMessageTrackablePart
    {
        public Queue<DiscordReactionRequest> ReactionRequests { get; } = new();

        public bool HasValue => ReactionRequests.Any();
        public bool UpdateNeeded { get; private set; }

        public void SetUpdateNeeded(bool value)
        {
            UpdateNeeded = value;
        }

        public void ResetContent()
        {
            ReactionRequests.Clear();
        }

        public void AddReaction(IEmote reaction)
        {
            ReactionRequests.Enqueue(new DiscordReactionRequest
            {
                Reaction = reaction,
                Type = DiscordReactionRequestType.Add
            });
            SetUpdateNeeded(true);
        }

        public void RemoveReaction(IEmote reaction)
        {
            ReactionRequests.Enqueue(new DiscordReactionRequest
            {
                Reaction = reaction,
                Type = DiscordReactionRequestType.RemoveSelf
            });
            SetUpdateNeeded(true);
        }

        public void RemoveAllReactionsOfType(IEmote reaction)
        {
            ReactionRequests.Enqueue(new DiscordReactionRequest
            {
                Reaction = reaction,
                Type = DiscordReactionRequestType.RemoveAllOfType
            });
            SetUpdateNeeded(true);
        }

        public void RemoveAllReactions(IEmote reaction)
        {
            ReactionRequests.Enqueue(new DiscordReactionRequest
            {
                Reaction = reaction,
                Type = DiscordReactionRequestType.RemoveAll
            });
            SetUpdateNeeded(true);
        }
    }
}