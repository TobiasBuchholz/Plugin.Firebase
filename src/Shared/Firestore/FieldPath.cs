namespace Plugin.Firebase.Firestore
{
    public sealed class FieldPath
    {
        private FieldPath(string[] fields = null, bool isDocumentId = false)
        {
            Fields = fields;
            IsDocumentId = isDocumentId;
        }

        public static FieldPath Of(string[] fields)
        {
            return new FieldPath(fields);
        }

        public static FieldPath DocumentId()
        {
            return new FieldPath(isDocumentId: true);
        }

        public string[] Fields { get; }
        public bool IsDocumentId { get; }
    }
}