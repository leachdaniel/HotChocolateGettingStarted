
namespace HotChocolateGettingStarted;


public class BookDataLoader : BatchDataLoader<int, Book>
{
    public BookDataLoader(IBatchScheduler batchScheduler, DataLoaderOptions? options = null) : base(batchScheduler, options)
    {
    }

    protected override async Task<IReadOnlyDictionary<int, Book>> LoadBatchAsync(IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        //await Task.Delay(10000);

        return keys.Select(x => new Book(x, $"Title {x}", x % 4)).ToDictionary(x => x.BookId);
    }
}

public class AuthorDataLoader : BatchDataLoader<int, Author>
{
    public AuthorDataLoader(IBatchScheduler batchScheduler, DataLoaderOptions? options = null) : base(batchScheduler, options)
    {
    }

    protected override async Task<IReadOnlyDictionary<int, Author>> LoadBatchAsync(IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        //await Task.Delay(10000);

        return keys.Select(x => new Author(x, $"Author {x}")).ToDictionary(x => x.AuthorId);
    }
}


public class ReviewDataLoader : BatchDataLoader<int, Review>
{
    public ReviewDataLoader(IBatchScheduler batchScheduler, DataLoaderOptions? options = null) : base(batchScheduler, options)
    {
    }

    protected override async Task<IReadOnlyDictionary<int, Review>> LoadBatchAsync(IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        //await Task.Delay(10000);

        return keys.Select(x => new Review(x, $"Review Content {x}", (x % 5) + 1)).ToDictionary(x => x.ReviewId);
    }
}

public class AuthorReviewByAuthorIdDataLoader : GroupedDataLoader<int, AuthorReview>
{
 
    public AuthorReviewByAuthorIdDataLoader(IBatchScheduler batchScheduler, DataLoaderOptions? options = null) : base(batchScheduler, options)
    {
    }

    protected override async Task<ILookup<int, AuthorReview>> LoadGroupedBatchAsync(IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        //await Task.Delay(10000);

        return keys
            .SelectMany(x => (Enumerable.Range(0, new Random(x).Next(1, 3))
                .Select(r => new AuthorReview(r + x, x))))
            .ToArray()
            .ToLookup(x => x.AuthorId);
    }
}

public class BookReviewByBookIdDataLoader : GroupedDataLoader<int, BookReview>
{

    public BookReviewByBookIdDataLoader(IBatchScheduler batchScheduler, DataLoaderOptions? options = null) : base(batchScheduler, options)
    {
    }

    protected override async Task<ILookup<int, BookReview>> LoadGroupedBatchAsync(IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        //await Task.Delay(10000);

        return keys
            .SelectMany(x => (Enumerable.Range(0, new Random(x).Next(3, 25))
                .Select(r => new BookReview(r + x, x))))
            .ToArray()
            .ToLookup(x => x.BookId);
    }
}