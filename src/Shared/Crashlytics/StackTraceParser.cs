using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace Plugin.Firebase.Crashlytics
{
    internal static class StackTraceParser
    {
        private static readonly Regex _regex = new Regex(@"^\s*at (?<className>\S+)\.(?<methodName>\S+ \(.*\)) (?<offset>.*?)( in (?<fileName>.+):(?<lineNumber>\d+))?\s*$",
            RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        public static IEnumerable<StackFrame> Parse(Exception exception)
        {
            var stackFrames = new List<StackFrame>();
            ParseException(exception, stackFrames);
            return stackFrames;
        }

        private static void ParseException(Exception exception, List<StackFrame> stackFrames)
        {
            if(exception == null) return;
            stackFrames.AddRange(ParseStackTrace(exception.StackTrace));
            if(exception is AggregateException aggregateException) {
                var number = 0;
                foreach(var innerException in aggregateException.InnerExceptions) {
                    ParseInnerException(innerException, number, stackFrames);
                    number++;
                }
            } else if(exception.InnerException != null) {
                ParseInnerException(exception.InnerException, 0, stackFrames);
            }
        }

        private static void ParseInnerException(Exception innerException, int number, List<StackFrame> stackFrames)
        {
            var (namespaceName, className) = GetNamespaceNameAndClassName(innerException.GetType());
            var methodName = string.Empty;
            if(!string.IsNullOrEmpty(namespaceName)) {
                methodName = $"{className}: {innerException.Message}";
                className = $"(Inner Exception #{number}) {namespaceName}";
            } else {
                className = $"(Inner Exception #{number}) {className}: {innerException.Message}";
            }
            stackFrames.Add(new StackFrame(className, methodName, "", 0));
            ParseException(innerException, stackFrames);
        }

        private static (string NamespaceName, string ClassName) GetNamespaceNameAndClassName(Type type)
        {
            var className = type.ToString();
            var namespaceName = type.Namespace;
            if(!string.IsNullOrEmpty(namespaceName)) {
                className = className.Substring(namespaceName.Length + 1);
            }
            return (namespaceName, className);
        }

        private static IEnumerable<StackFrame> ParseStackTrace(string stackTrace)
        {
            if(string.IsNullOrEmpty(stackTrace)) yield break;
            foreach(Match match in _regex.Matches(stackTrace)) {
                var className = match.Groups["className"].Value;
                var methodName = match.Groups["methodName"].Value;
                var lineNumberGroup = match.Groups["lineNumber"];
                var lineNumber = lineNumberGroup.Success ? int.Parse(lineNumberGroup.Value) : 0;
                var fileNameGroup = match.Groups["fileName"];
                var fileName = fileNameGroup.Success && lineNumber > 0 ? fileNameGroup.Value : "<unknown>";
                yield return new StackFrame(className, methodName, fileName, lineNumber);
            }
        }
    }
}