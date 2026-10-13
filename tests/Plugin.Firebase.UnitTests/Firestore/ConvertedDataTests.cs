using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.UnitTests;

public class ConvertedDataTests
{
    [Fact]
    public void converts_once_and_returns_the_same_instance_afterwards()
    {
        ConvertedData<object>? field = null;
        var conversions = 0;

        var first = ConvertedData<object>.GetOrConvert(ref field, "state", _ => {
            conversions++;
            return new object();
        });
        var second = ConvertedData<object>.GetOrConvert(ref field, "state", _ => {
            conversions++;
            return new object();
        });

        Assert.Same(first, second);
        Assert.Equal(1, conversions);
    }

    [Fact]
    public void keeps_converted_data_that_is_null()
    {
        ConvertedData<object>? field = null;
        var conversions = 0;

        ConvertedData<object>.GetOrConvert(ref field, "state", _ => {
            conversions++;
            return null;
        });
        var second = ConvertedData<object>.GetOrConvert(ref field, "state", _ => {
            conversions++;
            return new object();
        });

        Assert.Null(second);
        Assert.Equal(1, conversions);
    }

    [Fact]
    public void converts_again_after_a_failed_conversion()
    {
        ConvertedData<object>? field = null;
        var converted = new object();

        Assert.Throws<FormatException>(() => ConvertedData<object>.GetOrConvert<string>(ref field, "state", _ => throw new FormatException()));
        var result = ConvertedData<object>.GetOrConvert(ref field, "state", _ => converted);

        Assert.Same(converted, result);
    }

    [Fact]
    public async Task concurrent_first_reads_return_the_instance_stored_first()
    {
        var holder = new Holder();
        var conversions = 0;
        using var bothConverting = new Barrier(2);

        // both threads convert before either stores, so one of them has to hand back the other's instance
        object? Read() => ConvertedData<object>.GetOrConvert(ref holder.Field, bothConverting, barrier => {
            Interlocked.Increment(ref conversions);
            Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(10)), "the other thread never started converting");
            return new object();
        });
        var reads = await Task.WhenAll(Task.Run(Read), Task.Run(Read));

        Assert.Equal(2, conversions);
        Assert.Same(reads[0], reads[1]);
        Assert.Same(reads[0], ConvertedData<object>.GetOrConvert(ref holder.Field, bothConverting, _ => new object()));
    }

    private sealed class Holder
    {
        public ConvertedData<object>? Field;
    }
}