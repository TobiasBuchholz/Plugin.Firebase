using System;
using System.Collections.Generic;
using Utility.Builders;

namespace Playground.Common.Services.UserInteraction
{
    public sealed class UserInfoBuilder : IBuilder
    {
        private string _title;
        private string _message;
        private string _placeholder;
        private string _cancelButtonText;
        private string _destroyButtonText;
        private readonly IList<string> _defaultButtonTexts;
        private TimeSpan _snackbarDuration;
        private UserInfoType _type;

        public UserInfoBuilder()
        {
            _defaultButtonTexts = new List<string>();
            _snackbarDuration = TimeSpan.FromSeconds(5);
        }

        public UserInfoBuilder WithTitle(string title) =>
            this.With(ref _title, title);

        public UserInfoBuilder WithMessage(string message) =>
            this.With(ref _message, message);

        public UserInfoBuilder WithPlaceholder(string placeholder) =>
            this.With(ref _placeholder, placeholder);

        public UserInfoBuilder WithCancelButton(string text) =>
            this.With(ref _cancelButtonText, text);

        public UserInfoBuilder WithDestroyButtonText(string text) =>
            this.With(ref _destroyButtonText, text);

        public UserInfoBuilder WithDefaultButton(string text)
        {
            _defaultButtonTexts.Add(text);
            return this;
        }

        public UserInfoBuilder WithSnackbarDuration(TimeSpan duration) =>
            this.With(ref _snackbarDuration, duration);

        public UserInfoBuilder As(UserInfoType type) =>
            this.With(ref _type, type);

        public UserInfo Build() =>
            new UserInfo(_title, _message, _placeholder, _cancelButtonText, _destroyButtonText, _defaultButtonTexts, _snackbarDuration, _type);
    }

    public enum UserInfoType
    {
        Dialog, Snackbar, ActionSheet, Prompt
    }
}