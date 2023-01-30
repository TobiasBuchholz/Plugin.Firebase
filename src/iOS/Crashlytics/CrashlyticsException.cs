using System;
using System.Linq;
using Firebase.Crashlytics;

namespace Plugin.Firebase.Crashlytics
{
    internal class CrashlyticsException
    {
        public static ExceptionModel Create(Exception exception)
        {
            if(exception == null) throw new ArgumentNullException(nameof(exception));

            var exceptionModel = new ExceptionModel(exception.GetType().ToString(), exception.Message) {
                StackTrace = StackTraceParser.Parse(exception)
                    .Select(frame => new global::Firebase.Crashlytics.StackFrame(frame.Symbol, frame.FileName, frame.LineNumber))
                    .ToArray()
            };
            return exceptionModel;
        }
    }
}
