using System.Reflection;
using Plugin.Firebase.Core;

namespace Plugin.Firebase.UnitTests;

public class FirebaseInitializationHooksTests
{
    [Fact]
    public void register_before_configure_invokes_callbacks_in_order()
    {
        var called = new List<int>();

        using var first = FirebaseInitializationHooks.RegisterBeforeConfigure(() => called.Add(1));
        using var second = FirebaseInitializationHooks.RegisterBeforeConfigure(() => called.Add(2));

        InvokeInternal("InvokeBeforeConfigure");

        Assert.Equal(new[] { 1, 2 }, called);
    }

    [Fact]
    public void disposed_before_configure_callback_is_not_called()
    {
        var callCount = 0;
        var registration = FirebaseInitializationHooks.RegisterBeforeConfigure(() => callCount++);

        registration.Dispose();
        InvokeInternal("InvokeBeforeConfigure");

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void register_after_initialize_invokes_callbacks_in_order()
    {
        var called = new List<int>();

        using var first = FirebaseInitializationHooks.RegisterAfterInitialize(() => called.Add(1));
        using var second = FirebaseInitializationHooks.RegisterAfterInitialize(() => called.Add(2));

        InvokeInternal("InvokeAfterInitialize");

        Assert.Equal(new[] { 1, 2 }, called);
    }

    [Fact]
    public void disposed_after_initialize_callback_is_not_called()
    {
        var callCount = 0;
        var registration = FirebaseInitializationHooks.RegisterAfterInitialize(() => callCount++);

        registration.Dispose();
        InvokeInternal("InvokeAfterInitialize");

        Assert.Equal(0, callCount);
    }

    private static void InvokeInternal(string methodName)
    {
        var methodInfo = typeof(FirebaseInitializationHooks).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(methodInfo);
        methodInfo!.Invoke(null, null);
    }
}