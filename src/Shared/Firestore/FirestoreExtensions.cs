// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reactive.Linq;
// using Plugin.Firebase.Common;
// using Plugin.Firebase.Firestore;
//
// namespace Plugin.Firebase.Firestore
// {
//     public static class FirestoreExtensions
//     {
//         public static IObservable<IEnumerable<T>> AsObservable<T>(this ICollectionReference collectionReference) where T : IFirestoreObject<T>
//         {
//             return collectionReference
//                 .AsSnapshotObservable<T>()
//                 .Select(x => x.Documents.Select(y => y.Data.WithId(y.Reference.Id)));
//         }
//         
//         public static IObservable<IQuerySnapshot<T>> AsSnapshotObservable<T>(this ICollectionReference collectionReference)
//         {
//             return Observable
//                 .Create<IQuerySnapshot<T>>(observer => collectionReference.AddSnapshotListener<T>(observer.OnNext, observer.OnError));
//         }
//         
//         public static IObservable<IQuerySnapshot<T>> AsSnapshotObservable<T>(this IQuery query)
//         {
//             return Observable
//                 .Create<IQuerySnapshot<T>>(observer => query.AddSnapshotListener<T>(observer.OnNext, observer.OnError));
//         }
//         
//         public static IObservable<IDocumentSnapshot<T>> AsSnapshotObservable<T>(this IDocumentReference reference)
//         {
//             return Observable
//                 .Create<IDocumentSnapshot<T>>(observer => reference.AddSnapshotListener<T>(observer.OnNext, observer.OnError));
//         }
//     }
// }