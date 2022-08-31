using System;

namespace Playground.Common.Services.DynamicLink
{
    public interface IDynamicLinkService
    {
        IDisposable Register();
    }
}