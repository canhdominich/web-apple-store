namespace AppleStore.Web;

public static class ReadOnlyListExtensions
{
    public static int IndexOf<T>(this IReadOnlyList<T> source, T value) => source is IList<T> list ? list.IndexOf(value) : Enumerable.Range(0, source.Count).FirstOrDefault(i => EqualityComparer<T>.Default.Equals(source[i], value), -1);
}
