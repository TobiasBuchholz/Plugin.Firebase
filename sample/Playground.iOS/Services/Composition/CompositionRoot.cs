using Playground.Common.Services.Composition;
using Playground.Common.Services.UserInteraction;
using Playground.iOS.Services.UserInteraction;

namespace Playground.iOS.Services.Composition
{
    public sealed class CompositionRoot : CompositionRootBase
    {
        protected override IUserInteractionService CreateUserInteractionService() =>
            new UserInteractionService(_schedulerService.Value.Main);
    }
}