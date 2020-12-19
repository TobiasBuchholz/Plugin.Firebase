using System;

namespace Plugin.Firebase.Firestore
{
    public sealed class DocumentChange
    {
        public DocumentChange(
            IDocumentSnapshot documentSnapshot,
            DocumentChangeType changeType,
            int newIndex,
            int oldIndex)
        {
            DocumentSnapshot = documentSnapshot;
            ChangeType = changeType;
            NewIndex = newIndex;
            OldIndex = oldIndex;
        }

        public IDocumentSnapshot DocumentSnapshot { get; }
        public DocumentChangeType ChangeType { get; }
        public int NewIndex { get; }
        public int OldIndex { get; }
    }
}