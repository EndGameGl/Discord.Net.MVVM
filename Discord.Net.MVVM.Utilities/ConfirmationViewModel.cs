using System;
using System.Threading.Tasks;
using Discord.Net.MVVM.View.Controls;

namespace Discord.Net.MVVM.Utilities
{
    public class ConfirmationViewModel : DiscordViewModel
    {
        private readonly DiscordButton _confirmButton = new("confirm", "Accept")
        {
            IsControlActive = true
        };

        private readonly DiscordButton _declineButton = new("decline", "Decline")
        {
            IsControlActive = true,
            Style = ButtonStyle.Danger
        };

        private readonly string _text;
        private Action _onDecline;
        private Action _onSuccess;

        public ConfirmationViewModel(string text, Action onSuccess, Action onDecline)
        {
            _text = text;
            _onSuccess = onSuccess;
            _onDecline = onDecline;
        }

        public override DiscordEventBindings HandledEvents => DiscordEventBindings.InteractionCreated;

        public override async Task OnCreate()
        {
            ViewBody.Content.Modify(_text);
            ViewBody.Components.AddButton(_confirmButton, 0);
            ViewBody.Components.AddButton(_declineButton, 0);

            _confirmButton.OnClick += async _ => { _onSuccess?.Invoke(); };

            _declineButton.OnClick += async _ => { _onDecline?.Invoke(); };
        }

        public override ValueTask DisposeAsync()
        {
            _onSuccess = null;
            _onDecline = null;

            return ValueTask.CompletedTask;
        }
    }
}