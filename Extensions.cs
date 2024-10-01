namespace HotChocolateGettingStarted
{
    public static class DataLoaderExtensions
    {
        public static async Task<IReadOnlyDictionary<TKeySource, TValue>> LoadAsDictionary<TKey, TValue, TKeySource>(
            this IDataLoader<TKey, TValue> @this,
            IEnumerable<TKeySource> keys,
            CancellationToken cancellationToken = default) where TKeySource : TKey where TKey : notnull
        {
            var keysList = keys.Cast<TKey>().AsReadOnlyList();

            var results = await @this.LoadAsync(keysList, cancellationToken);

            var kvps = results
                .Where((_, i) => i < keysList.Count)
                .Select((x, i) => KeyValuePair.Create((TKeySource) keysList[i], x));

            return new Dictionary<TKeySource, TValue>(kvps);
        }

        public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> @this)
            => @this as IReadOnlyList<T> ?? @this?.ToArray() ?? Array.Empty<T>();
    }
}
