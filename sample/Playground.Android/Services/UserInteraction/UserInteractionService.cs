using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Android.Views;
using Google.Android.Material.Snackbar;
using Playground.Common.Services.UserInteraction;
using Plugin.CurrentActivity;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Playground.Droid.Services.UserInteraction
{
    public sealed class UserInteractionService : UserInteractionServiceBase
    {
        private readonly ICurrentActivity _current;

        public UserInteractionService(
            ICurrentActivity current,
            IScheduler mainScheduler = null)
            : base(mainScheduler)
        {
            _current = current;
        }

        protected override Task<int> ShowThreeButtonsDialogAsync(UserInfo userInfo)
        {
            var tcs = new TaskCompletionSource<int>();
            new AlertDialog
                .Builder(_current.Activity)
                .SetTitle(userInfo.Title)
                .SetMessage(userInfo.Message)
                .SetPositiveButton(userInfo.DefaultButtonTexts[0], (_,__) => tcs.SetResult(0))
                .SetNeutralButton(userInfo.DefaultButtonTexts[1], (_,__) => tcs.SetResult(1))
                .SetNegativeButton(userInfo.CancelButtonText ?? userInfo.DefaultButtonTexts[2], (_,__) => tcs.SetResult(DialogButtonIndex.Cancel))
                .Create()
                .Show();

            return tcs.Task;
        }

        protected override Task<int> ShowAsSnackbarImplAsync(UserInfo userInfo)
        {
            var tcs = new TaskCompletionSource<int>();
            var snackbar = Snackbar.Make(_current.Activity.FindViewById(Android.Resource.Id.Content), userInfo.Message, (int) userInfo.SnackbarDuration.TotalMilliseconds);
            AddDefaultButtonsToSnackbar(tcs, snackbar, userInfo.DefaultButtonTexts);
            SetResultToCancelledAfterDurationAsync(tcs, userInfo.SnackbarDuration).Ignore();
            snackbar.Show();
            return tcs.Task;
        }

        private static void AddDefaultButtonsToSnackbar(TaskCompletionSource<int> tcs, Snackbar snackbar, IList<string> defaultButtonTexts)
        {
            for(var i = 0; i < defaultButtonTexts.Count; i++) {
                snackbar.SetAction(defaultButtonTexts[i], CreateSnackbarAction(tcs, i));
            }
        }

        private static Action<View> CreateSnackbarAction(TaskCompletionSource<int> tcs, int index)
        {
            return x => tcs.TrySetResult(index);
        }
    }
}
