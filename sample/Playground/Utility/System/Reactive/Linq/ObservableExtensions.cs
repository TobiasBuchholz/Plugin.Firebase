using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Genesis.Logging;

namespace System.Reactive.Linq;

public static class ObservableExtensions
{
    public static IObservable<T> Debug<T>(this IObservable<T> source, Func<T, string> debugMessage)
    {
        return source.Do(t => Diagnostics.Debug.WriteLine(debugMessage(t)));
    }

    public static IObservable<T> DebugWriteLine<T>(this IObservable<T> source)
    {
        return source.Do(t => Diagnostics.Debug.WriteLine(t));
    }

    public static IObservable<bool> Negate(this IObservable<bool> source)
    {
        return source.Select(x => !x);
    }

    public static IObservable<T> CatchAndLogException<T>(
        this IObservable<T> source, T second,
        string message = null,
        [CallerFilePath] string callerFilePath = null)
    {
        return source.Catch<T, Exception>(e => LogCaughtException(e, Observable.Return(second), message, GetClassNameFromFilePath(callerFilePath)));
    }

    private static string GetClassNameFromFilePath(string filePath)
    {
        return filePath.Split('/').Last().Split('.').First();
    }

    public static IObservable<T> CatchAndLogException<T>(
        this IObservable<T> source,
        IObservable<T> second = null,
        string message = null,
        [CallerFilePath] string callerFilePath = null)
    {
        return source.Catch<T, Exception>(e => LogCaughtException(e, second, message, GetClassNameFromFilePath(callerFilePath)));
    }

    private static IObservable<T> LogCaughtException<T>(
        Exception e,
        IObservable<T> second = null,
        string message = null,
        string loggerName = nameof(ObservableExtensions))
    {
        LogException(e, message, loggerName);
        return second ?? Observable.Empty<T>();
    }

    private static void LogException(
        Exception e,
        string message = null,
        string loggerName = nameof(ObservableExtensions))
    {
        var logger = LoggerService.GetLogger(loggerName);
        logger.Error(e, message ?? "An exception was caught:\n");
    }

    public static IObservable<T> CatchAndIgnoreException<T>(this IObservable<T> source, IObservable<T> second = null)
    {
        return source.Catch<T, Exception>(e => second ?? Observable.Empty<T>());
    }

    public static IObservable<T> CatchAndLogParticularException<T>(
        this IObservable<T> source,
        Func<Exception, bool> predicate,
        IObservable<T> second = null,
        [CallerFilePath] string callerFilePath = null)
    {
        return source.Catch<T, Exception>(e => predicate(e)
            ? LogCaughtException<T>(e, second, GetClassNameFromFilePath(callerFilePath))
            : Observable.Throw<T>(e));
    }

    public static IObservable<Exception> LogThrownException(
        this IObservable<Exception> source,
        [CallerFilePath] string callerFilePath = null)
    {
        return source.Do(e => LogException(e, loggerName: GetClassNameFromFilePath(callerFilePath)));
    }

    public static IObservable<T> DoOnError<T>(this IObservable<T> source, Action<Exception> action)
    {
        return source.Do(_ => { }, action);
    }

    public static IObservable<T> DoOnCompleted<T>(this IObservable<T> source, Action action)
    {
        return source.Do(_ => { }, action);
    }

    public static IObservable<Unit> ToUnit<T>(this IObservable<T> @this)
    {
        return @this
            .Select(_ => Unit.Default);
    }

    public static IObservable<T> WhereNotNull<T>(this IObservable<T> source)
    {
        return source.Where(x => x != null);
    }

    public static IObservable<string> WhereNotEmpty(this IObservable<string> source)
    {
        return source.Where(x => !string.IsNullOrEmpty(x));
    }

    public static IObservable<bool> CombineLatestAsAnd(this IObservable<bool> @this, IObservable<bool> other)
    {
        return @this.CombineLatest(other, (x, y) => x && y);
    }

    public static IObservable<bool> CombineLatestAsOr(this IObservable<bool> @this, IObservable<bool> other)
    {
        return @this.CombineLatest(other, (x, y) => x || y);
    }

    public static IObservable<bool> WhereIsTrue(this IObservable<bool> source)
    {
        return source.Where(x => x);
    }

    public static IObservable<bool> WhereIsFalse(this IObservable<bool> source)
    {
        return source.Where(x => !x);
    }

    public static IObservable<T> WhereAsync<T>(this IObservable<T> source, Func<T, Task<bool>> predicate)
    {
        return source
            .SelectMany(async item => new { ShouldInclude = await predicate(item), Item = item })
            .Where(x => x.ShouldInclude)
            .Select(x => x.Item);
    }

    public static IObservable<T> PermaRef<T>(this IConnectableObservable<T> observable)
    {
        observable.Connect();
        return observable;
    }

    public static IObservable<bool> ToTrue<T>(this IObservable<T> @this)
    {
        return @this
            .Select(_ => true);
    }

    public static IObservable<bool> ToFalse<T>(this IObservable<T> @this)
    {
        return @this
            .Select(_ => false);
    }
}