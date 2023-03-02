namespace Plugin.Firebase.Firestore;

/// <summary>
/// Configures the behavior of <c>Get()</c> calls on <c>IDocumentReference</c> and <c>IQuery</c>. By providing a Source value, these methods
/// can be configured to fetch results only from the server, only from the local cache, or attempt to fetch results from the server and fall
/// back to the cache (which is the default).
/// </summary>
public enum Source
{
    Default, Cache, Server
}