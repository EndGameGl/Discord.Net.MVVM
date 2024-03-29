﻿using Discord.Net.MVVM.View.Controls;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Discord.Net.MVVM.View
{
    public sealed class DiscordTrackableComponent : IDiscordMessageTrackablePart
    {
        internal DiscordTrackableComponent()
        {
            for (var i = 0; i < 5; i++)
            {
                ActionRows[i] = new DiscordActionRow();
            }
        }

        public DiscordActionRow[] ActionRows { get; } = new DiscordActionRow[5];
        public ConcurrentDictionary<string, DiscordControl> ButtonMappings { get; } = new();

        public bool HasValue => !ButtonMappings.IsEmpty;
        public bool UpdateNeeded { get; private set; }

        public void SetUpdateNeeded(bool value)
        {
            UpdateNeeded = true;
        }

        public void ResetContent()
        {
            for (var i = 0; i < 5; i++)
            {
                ActionRows[i] = new DiscordActionRow();
            }
        }

        public MessageComponent BuildComponent()
        {
            var cb = new ComponentBuilder
            {
                ActionRows = new List<ActionRowBuilder>()
            };

            foreach (var row in ActionRows)
                if (row.IsBuildable())
                    cb.ActionRows.Add(row.GetBuilder());

            return cb.Build();
        }

        public void AddButton(DiscordControl button, int rowNumber)
        {
            ButtonMappings.TryAdd(button.Id, button);
            ActionRows[rowNumber].Controls.Add(button);
            SetUpdateNeeded(true);
        }

        public void RepositionButton(DiscordButton button, int newRowNumber)
        {
            for (var i = 0; i < 5; i++)
            {
                var actionRow = ActionRows[i];
                var indexOfButton = actionRow.Controls.IndexOf(button);
                if (indexOfButton != -1)
                {
                    actionRow.Controls.Remove(button);
                    ActionRows[newRowNumber].Controls.Add(button);
                    SetUpdateNeeded(true);
                    break;
                }
            }
        }
    }
}