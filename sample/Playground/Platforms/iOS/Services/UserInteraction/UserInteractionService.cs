using Playground.Common.Services.Scheduler;
using Playground.Common.Services.UserInteraction;
// using TTGSnackBar;
using UIKit;

namespace Playground.Platforms.iOS.Services.UserInteraction
{
    public sealed class UserInteractionService : UserInteractionServiceBase
    {
        public UserInteractionService(ISchedulerService schedulerService)
            : base(schedulerService)
        {
        }

        protected override Task<int> ShowThreeButtonsDialogAsync(UserInfo userInfo)
        {
            var tcs = new TaskCompletionSource<int>();
            var alert = UIAlertController.Create(userInfo.Title, userInfo.Message, UIAlertControllerStyle.Alert);
            AddDefaultButtonsToDialog(tcs, alert, userInfo.DefaultButtonTexts);
            AddCancelButtonToAlertIfNeeded(tcs, alert, userInfo.CancelButtonText);
            UIApplication.SharedApplication.KeyWindow?.RootViewController?.PresentViewController(alert, true, null);
            return tcs.Task;
        }

        private static void AddDefaultButtonsToDialog(TaskCompletionSource<int> tcs, UIAlertController alert, IList<string> defaultButtonTexts)
        {
            for(var i = 0; i < defaultButtonTexts.Count; i++) {
                alert.AddAction(CreateDefaultAction(tcs, i, defaultButtonTexts));
            }
        }

        private static UIAlertAction CreateDefaultAction(TaskCompletionSource<int> tcs, int index, IList<string> defaultButtonTexts)
        {
            return UIAlertAction.Create(defaultButtonTexts[index], UIAlertActionStyle.Default, _ => tcs.TrySetResult(index));
        }

        private static void AddCancelButtonToAlertIfNeeded(TaskCompletionSource<int> tcs, UIAlertController alert, string text)
        {
            if(!string.IsNullOrEmpty(text)) {
                alert.AddAction(UIAlertAction.Create(text, UIAlertActionStyle.Cancel, _ => tcs.TrySetResult(DialogButtonIndex.Cancel)));
            }
        }

        // protected override Task<int> ShowAsSnackbarImplAsync(UserInfo userInfo)
        // {
        //     var tcs = new TaskCompletionSource<int>();
        //     var snackbar = new TTGSnackbar(userInfo.Message);
        //     snackbar.Duration = userInfo.SnackbarDuration;
        //     snackbar.AnimationType = TTGSnackbarAnimationType.SlideFromBottomBackToBottom;
        //     snackbar.CornerRadius = 0f;
        //     snackbar.LeftMargin = 0f;
        //     snackbar.RightMargin = 0f;
        //     snackbar.BottomMargin = 0f;
        //     AddDefaultButtonsToSnackbar(tcs, snackbar, userInfo.DefaultButtonTexts);
        //     SetResultToCancelledAfterDurationAsync(tcs, userInfo.SnackbarDuration).Ignore();
        //     snackbar.Show();
        //     return tcs.Task;
        // }

        // private static void AddDefaultButtonsToSnackbar(TaskCompletionSource<int> tcs, TTGSnackbar snackbar, IList<string> defaultButtonTexts)
        // {
        //     for(var i = 0; i < defaultButtonTexts.Count; i++) {
        //         snackbar.ActionText = defaultButtonTexts[i];
        //         snackbar.ActionBlock = CreateSnackbarAction(tcs, i);
        //     }
        // }
        //
        // private static Action<TTGSnackbar> CreateSnackbarAction(TaskCompletionSource<int> tcs, int index)
        // {
        //     return x => tcs.TrySetResult(index);
        // }

        private static UIViewController RootViewController => UIApplication.SharedApplication.KeyWindow?.RootViewController;
    }
}