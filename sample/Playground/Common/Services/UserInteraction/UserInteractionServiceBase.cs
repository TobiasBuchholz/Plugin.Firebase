using Playground.Resources;

namespace Playground.Common.Services.UserInteraction;

// see https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/pop-ups for more options to implement
public abstract class UserInteractionServiceBase : IUserInteractionService
{
    private readonly ISchedulerService _schedulerService;

    protected UserInteractionServiceBase(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public Task<int> ShowAsDialogAsync(UserInfo userInfo)
    {
        return Observable
            .FromAsync(ct => ShowAsDialogImplAsync(userInfo))
            .SubscribeOn(_schedulerService.Main)
            .ToTask();
    }

    private async Task<int> ShowAsDialogImplAsync(UserInfo userInfo)
    {
        switch(userInfo.DefaultButtonTexts.Count) {
            case 1 when userInfo.CancelButtonText == null:
                await CurrentPage.DisplayAlert(userInfo.Title, userInfo.Message, userInfo.DefaultButtonTexts[0]);
                return 0;
            case 1 when userInfo.CancelButtonText != null:
                return await CurrentPage.DisplayAlert(userInfo.Title, userInfo.Message, userInfo.DefaultButtonTexts[0], userInfo.CancelButtonText) ? 1 : 0;
            case 2 when userInfo.CancelButtonText != null:
            case 3:
                return await ShowThreeButtonsDialogAsync(userInfo);
            case 2:
                return await CurrentPage.DisplayAlert(userInfo.Title, userInfo.Message, userInfo.DefaultButtonTexts[0], userInfo.DefaultButtonTexts[1]) ? 1 : 0;
            default:
                throw new ArgumentException("Too many buttons, use ShowAsActionSheetAsync() instead");
        }
    }

    protected abstract Task<int> ShowThreeButtonsDialogAsync(UserInfo userInfo);

    // public Task<int> ShowAsSnackbarAsync(UserInfo userInfo)
    // {
    //     return Observable
    //         .FromAsync(ct => ShowAsSnackbarImplAsync(userInfo))
    //         .SubscribeOn(_schedulerService.Main)
    //         .ToTask();
    // }

    // protected abstract Task<int> ShowAsSnackbarImplAsync(UserInfo userInfo);

    public Task<int> ShowAsActionSheetAsync(UserInfo userInfo)
    {
        return Observable
            .FromAsync(ct => ShowAsActionSheetImplAsync(userInfo))
            .SubscribeOn(_schedulerService.Main)
            .ToTask();
    }

    private static async Task<int> ShowAsActionSheetImplAsync(UserInfo userInfo)
    {
        var result = await CurrentPage.DisplayActionSheet(
            userInfo.Title,
            userInfo.CancelButtonText,
            userInfo.DestroyButtonText,
            userInfo.DefaultButtonTexts.ToArray());
        return userInfo.DefaultButtonTexts.IndexOf(result);
    }

    public Task<string> ShowAsPromptAsync(UserInfo userInfo)
    {
        return Observable
            .FromAsync(ct => ShowAsPromptImplAsync(userInfo))
            .SubscribeOn(_schedulerService.Main)
            .ToTask();
    }

    private Task<string> ShowAsPromptImplAsync(UserInfo userInfo)
    {
        return CurrentPage.DisplayPromptAsync(
            userInfo.Title,
            userInfo.Message,
            userInfo.DefaultButtonTexts[0],
            userInfo.CancelButtonText,
            userInfo.Placeholder,
            keyboard: Keyboard.Plain);
    }

    public Task ShowDefaultDialogAsync(string title, string message)
    {
        return ShowDefaultAsync(title, message, UserInfoType.Dialog);
    }

    private Task ShowDefaultAsync(string title, string message, UserInfoType type)
    {
        return ShowAsync(new UserInfoBuilder()
            .WithTitle(title)
            .WithMessage(message)
            .WithDefaultButton(Localization.Ok)
            .As(type)
            .Build());
    }

    public Task ShowDefaultSnackbarAsync(string message)
    {
        return ShowDefaultAsync(null, message, UserInfoType.Snackbar);
    }

    // public async Task ShowErrorSnackbarAsync(string message, Exception e = null)
    // {
    //     await ShowAsSnackbarAsync(
    //         new UserInfoBuilder()
    //             .WithMessage($"{message}: {e?.Message}")
    //             .WithDefaultButton(Localization.Ok)
    //             .As(UserInfoType.Snackbar)
    //             .Build());
    // }

    private Task<int> ShowAsync(UserInfo userInfo)
    {
        switch(userInfo.AsType) {
            case UserInfoType.Dialog:
                return ShowAsDialogAsync(userInfo);
            // case UserInfoType.Snackbar:
            // return ShowAsSnackbarAsync(userInfo);
            case UserInfoType.ActionSheet:
                return ShowAsActionSheetAsync(userInfo);
            default:
                throw new ArgumentException($"The given AsType {userInfo.AsType} is not yet supported");
        }
    }

    public async Task ShowErrorDialogAsync(string title, Exception e = null)
    {
        await ShowAsDialogAsync(
            new UserInfoBuilder()
                .WithTitle(title)
                .WithMessage(e?.Message)
                .WithDefaultButton(Localization.Ok)
                .As(UserInfoType.Dialog)
                .Build());
    }

    protected static async Task SetResultToCancelledAfterDurationAsync(TaskCompletionSource<int> tcs, TimeSpan duration)
    {
        await Task.Delay(duration);
        tcs.TrySetResult(DialogButtonIndex.Cancel);
    }

    private static Page CurrentPage => Application.Current.MainPage;
}

public static class DialogButtonIndex
{
    public const int Cancel = -1;
}