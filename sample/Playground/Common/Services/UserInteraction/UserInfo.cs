using System;
using System.Collections.Generic;
using System.Linq;

namespace Playground.Common.Services.UserInteraction
{
    public sealed class UserInfo
    {
        internal UserInfo(
            string title, 
            string message,
            string placeholder,
            string cancelButtonText,
            string destroyButtonText,
            IList<string> defaultButtonTexts,
            TimeSpan snackbarDuration,
            UserInfoType type)
        {
            Title = title;
            Message = message;
            Placeholder = Placeholder;
            CancelButtonText = cancelButtonText;
            DestroyButtonText = destroyButtonText;
            DefaultButtonTexts = defaultButtonTexts;
            SnackbarDuration = snackbarDuration;
            AsType = type;
        }
        
	    public override int GetHashCode()
        {
            return (Title, Message, Placeholder, CancelButtonText, DestroyButtonText, DefaultButtonTexts, SnackbarDuration, AsType).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is UserInfo other) {
                return Equals(other);
            }
            return false;
        }

        private bool Equals(UserInfo other)
        {
            return (Title, Message, Placeholder, CancelButtonText, DestroyButtonText, SnackbarDuration, AsType)
                .Equals((other.Title, other.Message, other.Placeholder, other.CancelButtonText, other.DestroyButtonText, other.SnackbarDuration, other.AsType)) && 
                DefaultButtonTexts.SequenceEqualSafe(other.DefaultButtonTexts);
        }

        public string Title { get; }
        public string Message { get; }
        public string Placeholder { get; }
        public string CancelButtonText { get; }
        public string DestroyButtonText { get; }
        public IList<string> DefaultButtonTexts { get; }
        public TimeSpan SnackbarDuration { get; }
        public UserInfoType AsType { get; }
    }
}