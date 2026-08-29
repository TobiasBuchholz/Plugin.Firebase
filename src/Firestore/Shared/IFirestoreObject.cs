using System.Diagnostics.CodeAnalysis;

namespace Plugin.Firebase.Firestore;

/// <summary>
/// Marker interface for objects that can be serialized to and from Firestore documents.
/// </summary>
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
    | DynamicallyAccessedMemberTypes.PublicProperties
)]
public interface IFirestoreObject { }