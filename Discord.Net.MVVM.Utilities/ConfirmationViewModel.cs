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
        private Func<Task> _onDecline;
        private Func<Task> _onSuccess;

        public override bool DisposeOnMessageDeletion => true;

        public ConfirmationViewModel(
            string text,
            Func<Task> onSuccess,
            Func<Task> onDecline)
        {
            _text = text;
            _onSuccess = onSuccess;
            _onDecline = onDecline;
        }

        public override Task OnCreate()
        {
            ModifyText(_text);
            AddButton(_confirmButton, 0);
            AddButton(_declineButton, 0);

            _confirmButton.OnClick += async _ => { await _onSuccess?.Invoke(); };

            _declineButton.OnClick += async _ => { await _onDecline?.Invoke(); };

            return Task.CompletedTask;
        }

        public override ValueTask DisposeAsync()
        {
            _onSuccess = null;
            _onDecline = null;

            return ValueTask.CompletedTask;
        }
    }
}