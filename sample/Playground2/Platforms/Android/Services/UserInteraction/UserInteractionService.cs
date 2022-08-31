using Playground.Common.Services.Scheduler;
using Playground.Common.Services.UserInteraction;
using Plugin.CurrentActivity;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using NativeSnackbar = Google.Android.Material.Snackbar.Snackbar;
using NativeView = Android.Views.View;

namespace Playground.Platforms.Android.Services.UserInteraction
{
    public sealed class UserInteractionService : UserInteractionServiceBase
    {
        private readonly ICurrentActivity _current;

        public UserInteractionService(
            ICurrentActivity current,
            ISchedulerService schedulerService)
            : base(schedulerService)
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

        // protected override Task<int> ShowAsSnackbarImplAsync(UserInfo userInfo)
        // {
        //     var tcs = new TaskCompletionSource<int>();
        //     var snackbar = NativeSnackbar.Make(_current.Activity.FindViewById(global::Android.Resource.Id.Content), userInfo.Message, (int) userInfo.SnackbarDuration.TotalMilliseconds);
        //     AddDefaultButtonsToSnackbar(tcs, snackbar, userInfo.DefaultButtonTexts);
        //     SetResultToCancelledAfterDurationAsync(tcs, userInfo.SnackbarDuration).Ignore();
        //     snackbar.Show();
        //     return tcs.Task;
        // }
        //
        // private static void AddDefaultButtonsToSnackbar(TaskCompletionSource<int> tcs, NativeSnackbar snackbar, IList<string> defaultButtonTexts)
        // {
        //     for(var i = 0; i < defaultButtonTexts.Count; i++) {
        //         snackbar.SetAction(defaultButtonTexts[i], CreateSnackbarAction(tcs, i));
        //     }
        // }
        //
        // private static Action<NativeView> CreateSnackbarAction(TaskCompletionSource<int> tcs, int index)
        // {
        //     return x => tcs.TrySetResult(index);
        // }
    }
}
