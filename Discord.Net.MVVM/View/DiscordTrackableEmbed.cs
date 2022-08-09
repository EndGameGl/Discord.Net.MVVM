namespace Discord.Net.MVVM.View
{
    public sealed class DiscordTrackableEmbed : IDiscordMessageTrackablePart
    {
        public Embed? Embed { get; private set; }
        public bool HasValue => Embed is not null;
        public bool UpdateNeeded { get; private set; }

        internal DiscordTrackableEmbed()
        {
            Embed = null;
        }

        public void SetUpdateNeeded(bool value)
        {
            UpdateNeeded = value;
        }

        public void ResetContent()
        {
            ModifyContent(null);
        }

        public void ModifyContent(Embed? value)
        {
            if (Embed is not null && value is null)
            {
                SetUpdateNeeded(true);
                Embed = null;
                return;
            }

            if (Embed is null && value is not null)
            {
                Embed = value;
                SetUpdateNeeded(true);
                return;
            }

            if (Embed is null && value is null)
                return;

            if (!AreEmbedsEqual(Embed!, value!))
            {
                Embed = value;
                SetUpdateNeeded(true);
            }
        }

        private bool AreEmbedsEqual(Embed first, Embed second)
        {
            if (first.Length != second.Length)
                return false;

            #region Author property check

            if (first.Author.HasValue != second.Author.HasValue)
                return false;
            if (first.Author.HasValue && second.Author.HasValue)
            {
                var firstAuthorValue = first.Author.Value;
                var secondAuthorValue = second.Author.Value;
                if (firstAuthorValue.Name != secondAuthorValue.Name)
                    return false;
                if (firstAuthorValue.Url != secondAuthorValue.Url)
                    return false;
                if (firstAuthorValue.IconUrl != secondAuthorValue.IconUrl)
                    return false;
                if (firstAuthorValue.ProxyIconUrl != secondAuthorValue.ProxyIconUrl)
                    return false;
            }

            #endregion

            #region Color property check

            if (first.Color.HasValue != second.Color.HasValue)
                return false;
            if (first.Color.HasValue && second.Color.HasValue)
            {
                var firstColorValue = first.Color.Value;
                var secondColorValue = second.Color.Value;
                if (firstColorValue.RawValue != secondColorValue.RawValue)
                    return false;
            }

            #endregion

            if (first.Description != second.Description)
                return false;

            #region Fields property check

            if (first.Fields.Length != second.Fields.Length)
                return false;

            for (var i = 0; i < first.Fields.Length; i++)
            {
                var firstField = first.Fields[i];
                var secondField = second.Fields[i];
                if (firstField.Inline != secondField.Inline)
                    return false;
                if (firstField.Name != secondField.Name)
                    return false;
                if (firstField.Value != secondField.Value)
                    return false;
            }

            #endregion

            #region Footer property check

            if (first.Footer.HasValue != second.Footer.HasValue)
                return false;

            if (first.Footer.HasValue && second.Footer.HasValue)
            {
                var firstFooterValue = first.Footer.Value;
                var secondFooterValue = second.Footer.Value;
                if (firstFooterValue.Text != secondFooterValue.Text)
                    return false;
                if (firstFooterValue.ProxyUrl != secondFooterValue.ProxyUrl)
                    return false;
                if (firstFooterValue.IconUrl != secondFooterValue.IconUrl)
                    return false;
            }

            #endregion

            #region Image property check

            if (first.Image.HasValue != second.Image.HasValue)
                return false;

            if (first.Image.HasValue && second.Image.HasValue)
            {
                var firstImageValue = first.Image.Value;
                var secondImageValue = second.Image.Value;

                if (firstImageValue.Height != secondImageValue.Height)
                    return false;
                if (firstImageValue.Width != secondImageValue.Width)
                    return false;
                if (firstImageValue.Url != secondImageValue.Url)
                    return false;
                if (firstImageValue.ProxyUrl != secondImageValue.ProxyUrl)
                    return false;
            }

            #endregion

            #region Provider property check

            if (first.Provider.HasValue != second.Provider.HasValue)
                return false;

            if (first.Provider.HasValue && second.Provider.HasValue)
            {
                var firstProviderValue = first.Provider.Value;
                var secondProviderValue = second.Provider.Value;

                if (firstProviderValue.Name != secondProviderValue.Name)
                    return false;
                if (firstProviderValue.Url != secondProviderValue.Url)
                    return false;
            }

            #endregion

            #region Thumbnail property check

            if (first.Thumbnail.HasValue != second.Thumbnail.HasValue)
                return false;

            if (first.Thumbnail.HasValue && second.Thumbnail.HasValue)
            {
                var firstThumbnailValue = first.Thumbnail.Value;
                var secondThumbnailValue = second.Thumbnail.Value;

                if (firstThumbnailValue.Height != secondThumbnailValue.Height)
                    return false;
                if (firstThumbnailValue.Url != secondThumbnailValue.Url)
                    return false;
                if (firstThumbnailValue.Width != secondThumbnailValue.Width)
                    return false;
                if (firstThumbnailValue.ProxyUrl != secondThumbnailValue.ProxyUrl)
                    return false;
            }

            #endregion

            if (first.Timestamp != second.Timestamp)
                return false;

            if (first.Title != second.Title)
                return false;

            if (first.Type != second.Type)
                return false;

            if (first.Url != second.Url)
                return false;

            #region Video property check

            if (first.Video.HasValue != second.Video.HasValue)
                return false;

            if (first.Video.HasValue && second.Video.HasValue)
            {
                var firstVideoValue = first.Video.Value;
                var secondVideoValue = second.Video.Value;

                if (firstVideoValue.Height != secondVideoValue.Height)
                    return false;
                if (firstVideoValue.Width != secondVideoValue.Width)
                    return false;
                if (firstVideoValue.Url != secondVideoValue.Url)
                    return false;
            }

            #endregion

            return true;
        }
    }
}