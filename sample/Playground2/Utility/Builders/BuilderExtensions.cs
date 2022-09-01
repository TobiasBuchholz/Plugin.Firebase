namespace Utility.Builders;

public static class BuilderExtensions
{
    public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref TField field, TField value) where TBuilder : IBuilder
    {
        field = value;
        return @this;
    }

    public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField> field, IEnumerable<TField> values) where TBuilder : IBuilder
    {
        if(values == null) {
            field = null;
        } else {
            field.AddRange(values);
        }
        return @this;
    }

    public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField> field, TField value) where TBuilder : IBuilder
    {
        field.Add(value);
        return @this;
    }

    public static TBuilder Without<TBuilder, TField>(this TBuilder @this, ref List<TField> field, Predicate<TField> predicate) where TBuilder : IBuilder
    {
        field.RemoveAll(predicate);
        return @this;
    }

    public static TBuilder WithoutAll<TBuilder, TField>(this TBuilder @this, ref List<TField> field) where TBuilder : IBuilder
    {
        field.Clear();
        return @this;
    }
}