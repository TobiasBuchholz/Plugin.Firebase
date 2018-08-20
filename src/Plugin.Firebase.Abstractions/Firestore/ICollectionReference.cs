using System.Threading.Tasks;

namespace Plugin.Firebase.Abstractions.Firestore
{
    public interface ICollectionReference
    {
        IDocumentReference GetDocument(string documentPath);
        IDocumentReference CreateDocument();
        Task<IDocumentReference> AddDocumentAsync(object data);
    }
}