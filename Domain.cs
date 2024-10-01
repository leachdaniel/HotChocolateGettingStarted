namespace HotChocolateGettingStarted;

public record Book(int BookId, string Title, int AuthorId);

public record Author(int AuthorId, string Name);

public record Review(int ReviewId, string Content, int Rating);

public record AuthorReview(int ReviewId, int AuthorId);

public record BookReview(int ReviewId, int BookId);    
