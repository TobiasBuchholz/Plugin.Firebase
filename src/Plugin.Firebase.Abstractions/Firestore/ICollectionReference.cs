namespace Plugin.Firebase.Abstractions.Firestore
{
    public interface ICollectionReference
    {
        IDocumentReference GetDocument(string documentPath);
    }
}