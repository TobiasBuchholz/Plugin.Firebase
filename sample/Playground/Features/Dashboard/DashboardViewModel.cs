using System.Reactive;
using Playground.Common.Base;
using Playground.Common.Services.Navigation;
using ReactiveUI;

namespace Playground.Features.Dashboard
{
    public sealed class DashboardViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        
        public DashboardViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            InitCommands();
        }

        private void InitCommands()
        {
            NavigateToRemoteConfigPageCommand = ReactiveCommand.CreateFromTask(() => _navigationService.GoToAsync(NavigationPaths.ToRemoteConfigPage()));
            NavigateToStoragePageCommand = ReactiveCommand.CreateFromTask(() => _navigationService.GoToAsync(NavigationPaths.ToStoragePage()));
        }

        public ReactiveCommand<Unit, Unit> NavigateToRemoteConfigPageCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> NavigateToStoragePageCommand { get; private set; }
    }
}