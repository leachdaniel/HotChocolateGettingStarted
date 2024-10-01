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
    performance problems: 
    takes about 160ms

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

