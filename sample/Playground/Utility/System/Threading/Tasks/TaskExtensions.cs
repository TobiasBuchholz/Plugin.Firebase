namespace System.Threading.Tasks;

public static class TaskExtensions
{
    public static void Ignore(this Task @this)
    {
        // ¯\_(ツ)_/¯
    }

    public static void Ignore<T>(this Task<T> @this)
    {
        // ¯\_(ツ)_/¯
    }
}