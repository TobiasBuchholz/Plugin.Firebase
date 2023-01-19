namespace Plugin.Firebase.Crashlytics
{
    internal class StackFrame
    {
        public string ClassName { get; }
        public string MethodName { get; }
        public string FileName { get; }
        public int LineNumber { get; }

        public string Symbol => string.IsNullOrEmpty(MethodName) ? ClassName : $"{ClassName}.{MethodName}";

        public StackFrame(string className, string methodName, string fileName, int lineNumber)
        {
            ClassName = className;
            MethodName = methodName;
            FileName = fileName;
            LineNumber = lineNumber;
        }
    }
}