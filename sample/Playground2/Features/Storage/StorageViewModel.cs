using System.Text;
using Plugin.Firebase.Storage;

namespace Playground.Features.Storage;

[Preserve(AllMembers = true)]
public sealed class StorageViewModel : ViewModelBase
{
    private readonly IUserInteractionService _userInteractionService;
    private readonly IFirebaseStorage _firebaseStorage;

    public StorageViewModel(
        IUserInteractionService userInteractionService,
        IFirebaseStorage firebaseStorage)
    {
        _userInteractionService = userInteractionService;
        _firebaseStorage = firebaseStorage;

        InitCommands();
    }

    private void InitCommands()
    {
        UploadTextCommand = ReactiveCommand.CreateFromTask(UploadTextAsync);

        UploadTextCommand
            .ThrownExceptions
            .LogThrownException()
            .Subscribe(e => _userInteractionService.ShowErrorDialogAsync(Localization.DialogTitleUnexpectedError, e))
            .DisposeWith(Disposables);
    }

    private Task UploadTextAsync()
    {
        return _firebaseStorage
            .GetRootReference()
            .GetChild($"uploads/{DateTime.Now}.txt")
            .PutBytes(Encoding.UTF8.GetBytes(Text))
            .AwaitAsync();
    }

    [Reactive] public string Text { get; set; }
    public ReactiveCommand<Unit, Unit> UploadTextCommand { get; set; }
}