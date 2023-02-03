using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Playground.Common.Base;

public abstract class ContentPageBase : ContentPage
{
    protected static Func<bool, bool> Negate => x => !x;

    protected ContentPageBase()
    {
        On<iOS>().SetUseSafeArea(true);
    }
}