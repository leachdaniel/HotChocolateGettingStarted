using System.Runtime.InteropServices;
using HotChocolate.Execution.Processing;
using HotChocolate.Resolvers;

namespace HotChocolateGettingStarted.Types;

[QueryType]
public static class Query
{
    public static async Task<Book> GetBook(int bookId, [Service] BookDataLoader bookDataLoader, CancellationToken cancellationToken)
    {
        var book = await bookDataLoader.LoadAsync(bookId, cancellationToken);

        return book;
    }

    /*
    performance problems when using DataLoader relationships
    takes about 160ms in debug
    takes 15-20ms regular

    using ResolverContext to set a state
    debug mode takes 14-20ms

    regular mode takes 4-10ms



    query {

    books(bookIds: [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99]) {
        bookId
        reviews {
            reviewId
            rating
            content
        }
        author {
            name
            reviews {
                reviewId
                rating
                content
            }
        }
    }
}
     */

    //[IsSelected("reviews")]
    public static async Task<IEnumerable<Book>> GetBooks(IReadOnlyCollection<int> bookIds, IResolverContext context, [Service] BookDataLoader bookDataLoader, CancellationToken cancellationToken)
    {

        var selections = context.GetSelections((ObjectType)context.Selection.Type.NamedType());

        var loadAndSetBookReviewsTask = LoadAndSetBookReviews(bookIds, context, cancellationToken);

        var books = await bookDataLoader.LoadAsync(bookIds, cancellationToken);

        var loadAuthorsAndReviewsTask = LoadAndSetAuthorsAndAuthorReviews(books, context, selections, cancellationToken);

        await Task.WhenAll(loadAuthorsAndReviewsTask, loadAndSetBookReviewsTask);

        return books;
    }

    private static async Task LoadAndSetAuthorsAndAuthorReviews(IReadOnlyCollection<Book> books, IResolverContext context, IReadOnlyList<ISelection> selections, CancellationToken cancellationToken)
    {
        var authorField = selections.FirstOrDefault(s => s.Field.Name.Equals("author"));
        var isAuthorSelected = authorField is not null;

        var authorsTask = Task.FromResult(
            (IReadOnlyDictionary<int, Author>) Array.Empty<Author>()
                .ToDictionary(_ => 0));

        var authorReviewsTask = Task.CompletedTask;

        if (isAuthorSelected)
        {
            var authorIds = books.Select(x => x.AuthorId)
                .ToHashSet();

            authorsTask = context.Service<AuthorDataLoader>()
                .LoadAsDictionary(authorIds, cancellationToken);

            var authorSelections = context.GetSelections((ObjectType) authorField!.Type.NamedType(), authorField);

            var isAuthorReviewsSelected = authorSelections.Any(x => x.Field.Name.Equals("reviews"));

            if (isAuthorReviewsSelected)
            {
                authorReviewsTask = LoadAndSetAuthorReviews(authorsTask, context, cancellationToken);
            }

            context.SetScopedState("AuthorsByAuthorId", await authorsTask);
        }

        await Task.WhenAll(authorReviewsTask, authorsTask);
    }

    private static async Task LoadAndSetAuthorReviews(Task<IReadOnlyDictionary<int, Author>> authorsTask, IResolverContext context, CancellationToken cancellationToken)
    {
        var authors = await authorsTask;

        var authorIds = authors.Keys.AsReadOnlyList();

        var authorReviews = await context.Service<AuthorReviewByAuthorIdDataLoader>().LoadAsync(authorIds, cancellationToken);

        var reviewIds = authorReviews.SelectMany(x => x.Select(r => r.ReviewId)).ToHashSet();

        var reviews = await context.Service<ReviewDataLoader>().LoadAsDictionary(reviewIds, cancellationToken);

        var reviewsByAuthorIdLookup = authorReviews
            .SelectMany(x => x.Select(rl => new { rl, r = reviews.GetValueOrDefault(rl.ReviewId) }))
            .Where(x => x.r is not null)
            .ToLookup(x => x.rl.AuthorId, x => x.r);

        context.SetScopedState("ReviewsByAuthorId", reviewsByAuthorIdLookup);
    }

    private static async Task LoadAndSetBookReviews(IReadOnlyCollection<int> bookIds, IResolverContext context, CancellationToken cancellationToken)
    {
        var bookReviews = await context.Service<BookReviewByBookIdDataLoader>().LoadAsync(bookIds, cancellationToken);

        var reviewIds = bookReviews.SelectMany(x => x.Select(r => r.ReviewId)).ToHashSet();

        var reviews = await context.Service<ReviewDataLoader>().LoadAsDictionary(reviewIds, cancellationToken);

        var reviewsByAuthorIdLookup = bookReviews
            .SelectMany(x => x.Select(rl => new { rl, r = reviews.GetValueOrDefault(rl.ReviewId) }))
            .Where(x => x.r is not null)
            .ToLookup(x => x.rl.BookId, ar => ar.r);

        context.SetScopedState("ReviewsByBookId", reviewsByAuthorIdLookup);
    }
}


[ExtendObjectType<Book>]
public static class BookRelationships
{

    public static IEnumerable<Review> GetReviews(
        [Parent] Book book,
        IResolverContext context,
        CancellationToken cancellationToken)
    {
        var reviewLookup = context.GetScopedState<ILookup<int, Review>>("ReviewsByBookId");

        return reviewLookup[book.BookId];
    }

    private static async Task<IEnumerable<Review>> GetReviewsWithDataLoader(
        [Parent] Book book, 
        [Service] BookReviewByBookIdDataLoader bookReviewByBookIdDataLoader,
        [Service] ReviewDataLoader reviewDataLoader,
        CancellationToken cancellationToken)
    {
        var reviewRelationships = await bookReviewByBookIdDataLoader.LoadAsync(book.BookId, cancellationToken);

        var reviewIds = reviewRelationships.Select(x => x.ReviewId).ToHashSet();

        var reviews = await reviewDataLoader.LoadAsync(reviewIds, cancellationToken);

        return reviews;
    }

    public static Author GetAuthor(
        [Parent] Book book,
        IResolverContext context,
        CancellationToken cancellationToken)
    {
        var authors = context.GetScopedState<IReadOnlyDictionary<int, Author>>("AuthorsByAuthorId");

        return authors.GetValueOrDefault(book.AuthorId);
    }

    private static async Task<Author> GetAuthorWithDataLoader(
        [Parent] Book book,
        [Service] AuthorDataLoader authorDataLoader,
        CancellationToken cancellationToken)
    {
        var author = await authorDataLoader.LoadAsync(book.AuthorId, cancellationToken);

        return author;
    }
}

[ExtendObjectType<Author>]
public static class AuthorRelationships
{

    private static async Task<IEnumerable<Review>> GetReviewsWithDataLoaders(
        [Parent] Author author,
        [Service] AuthorReviewByAuthorIdDataLoader authorReviewByAuthorIdDataLoader,
        [Service] ReviewDataLoader reviewDataLoader,
        CancellationToken cancellationToken)
    {
        var reviewRelationships = await authorReviewByAuthorIdDataLoader.LoadAsync(author.AuthorId, cancellationToken);

        var reviewIds = reviewRelationships.Select(x => x.ReviewId).ToHashSet();

        var reviews = await reviewDataLoader.LoadAsync(reviewIds, cancellationToken);

        return reviews;
    }

    public static IEnumerable<Review> GetReviews(
        [Parent] Author author,
        IResolverContext context,
        CancellationToken cancellationToken)
    {
        var reviewLookup = context.GetScopedState<ILookup<int, Review>>("ReviewsByAuthorId");

        return reviewLookup[author.AuthorId];
    }
}

