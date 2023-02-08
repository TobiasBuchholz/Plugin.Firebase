using Java.Lang;

namespace Plugin.Firebase.Crashlytics;

internal class CrashlyticsException : Java.Lang.Exception
{
    public CrashlyticsException(string message, StackTraceElement[] stackTrace) : base(message)
    {
        SetStackTrace(stackTrace);
    }

    public static CrashlyticsException Create(System.Exception exception)
    {
        if(exception == null) throw new ArgumentNullException(nameof(exception));

        var message = $"{exception.GetType()}: {exception.Message}";

        var stackTrace = StackTraceParser.Parse(exception)
            .Select(frame => new StackTraceElement(frame.ClassName, frame.MethodName, frame.FileName, frame.LineNumber))
            .ToArray();

        return new CrashlyticsException(message, stackTrace);
    }
}