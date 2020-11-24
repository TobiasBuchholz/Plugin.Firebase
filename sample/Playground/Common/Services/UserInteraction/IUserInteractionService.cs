using System;
using System.Threading.Tasks;

namespace Playground.Common.Services.UserInteraction
{
    public interface IUserInteractionService
    {
        Task<int> ShowAsDialogAsync(UserInfo userInfo);
        Task<int> ShowAsSnackbarAsync(UserInfo userInfo);
        Task<int> ShowAsActionSheetAsync(UserInfo userInfo);
        Task<string> ShowAsPromptAsync(UserInfo userInfo);

        Task ShowDefaultDialogAsync(string title, string message);
        Task ShowDefaultSnackbarAsync(string message);
        Task ShowErrorDialogAsync(string title, Exception e = null);
        Task ShowErrorSnackbarAsync(string message, Exception e = null);
    }
}