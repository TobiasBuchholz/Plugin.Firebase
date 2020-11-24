using Playground.Common.Services.Composition;
using Playground.Common.Services.UserInteraction;
using Playground.Droid.Services.UserInteraction;
using Plugin.CurrentActivity;

namespace Playground.Droid.Services.Composition
{
    public sealed class CompositionRoot : CompositionRootBase
    {
        protected override IUserInteractionService CreateUserInteractionService() =>
            new UserInteractionService(CrossCurrentActivity.Current, _schedulerService.Value.Main);
    }
}