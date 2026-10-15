using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.UnitTests;

public class TransactionResultGuardTests
{
    public static TheoryData<Type> AsyncResultTypes => new() {
        typeof(Task),
        typeof(Task<int>),
        typeof(ValueTask),
        typeof(ValueTask<string>)
    };

    public static TheoryData<Type> SynchronousResultTypes => new() {
        typeof(int),
        typeof(string),
        typeof(object),
        typeof(DateTime),
        typeof(string[]),
        typeof(ValueTuple<string, long>),
        typeof(HashSet<string>),
        // a task can still be returned through these, which the value check catches
        typeof(IAsyncResult),
        typeof(IDisposable)
    };

    [Theory]
    [MemberData(nameof(AsyncResultTypes))]
    public void rejects_task_result_types_for_the_update_function(Type resultType)
    {
        var exception = Assert.Throws<ArgumentException>(() => TransactionResultGuard.ThrowIfAsyncResultType(resultType));

        Assert.Equal("updateFunc", exception.ParamName);
    }

    [Fact]
    public void rejects_the_result_type_inferred_for_an_async_lambda()
    {
        Assert.Throws<ArgumentException>(() => ThrowIfAsyncResultTypeOf(async _ => await Task.Yield()));
        Assert.Throws<ArgumentException>(() => ThrowIfAsyncResultTypeOf(async _ => {
            await Task.Yield();
            return 1;
        }));
    }

    [Theory]
    [MemberData(nameof(SynchronousResultTypes))]
    public void accepts_synchronous_result_types(Type resultType)
    {
        TransactionResultGuard.ThrowIfAsyncResultType(resultType);
    }

    [Fact]
    public void rejects_task_results_returned_through_other_result_types()
    {
        Assert.Equal("updateFunc", TransactionResultGuard.GetAsyncResultException(Task.CompletedTask)?.ParamName);
        Assert.NotNull(TransactionResultGuard.GetAsyncResultException(Task.FromResult(1)));
        Assert.NotNull(TransactionResultGuard.GetAsyncResultException(new ValueTask()));
        Assert.NotNull(TransactionResultGuard.GetAsyncResultException(new ValueTask<int>(1)));
        // an async method returns a subclass of Task<T>
        Assert.NotNull(TransactionResultGuard.GetAsyncResultException(YieldAsync()));
    }

    [Fact]
    public void accepts_synchronous_results()
    {
        Assert.Null(TransactionResultGuard.GetAsyncResultException(null));
        Assert.Null(TransactionResultGuard.GetAsyncResultException(5));
        Assert.Null(TransactionResultGuard.GetAsyncResultException("result"));
        Assert.Null(TransactionResultGuard.GetAsyncResultException(("Charmander", 1L)));
        Assert.Null(TransactionResultGuard.GetAsyncResultException(new[] { "ember" }));
    }

    private static void ThrowIfAsyncResultTypeOf<TResult>(Func<object, TResult> updateFunc)
    {
        TransactionResultGuard.ThrowIfAsyncResultType(typeof(TResult));
    }

    private static async Task<int> YieldAsync()
    {
        await Task.Yield();
        return 1;
    }
}