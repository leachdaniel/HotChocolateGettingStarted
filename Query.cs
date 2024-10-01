namespace HotChocolateGettingStarted.Types;

[QueryType]
public static class Query
{
    public static async Task<Book> GetBook(int bookId, [Service] BookDataLoader bookDataLoader, CancellationToken cancellationToken)
    {
        var book = await bookDataLoader.LoadAsync(bookId, cancellationToken);

        return book;
    }

    public static async Task<IEnumerable<Book>> GetBooks(IReadOnlyCollection<int> bookIds, [Service] BookDataLoader bookDataLoader, CancellationToken cancellationToken)
    {
        var books = await bookDataLoader.LoadAsync(bookIds, cancellationToken);

        return books;
    }
}


[ExtendObjectType<Book>]
public static class BookRelationships
{

    public static async Task<IEnumerable<Review>> GetReviews(
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

    public static async Task<Author> GetAuthor(
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

    public static async Task<IEnumerable<Review>> GetReviews(
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
}

