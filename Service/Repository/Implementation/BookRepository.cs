using System.Linq.Expressions;
using AutoMapper;
using Common;
using Models.Book;
using Models.RequestModel;
using Models.ResponseModel;
using Models.SpDbContext;
using Newtonsoft.Json;
using Serilog;
using Service.Repository.Interface;
using Service.RepositoryFactory;
using Service.UnitOfWork;

namespace Service.Repository.Implementation;

public class BookRepository : IBookRepository
{
    private readonly BookDbContext _context;
    private readonly LibraryManagementSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BookRepository(BookDbContext context, LibraryManagementSpContext spContext, IUnitOfWork unitOfWork)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BookResponseModel>> List(Dictionary<string, object> parameters)
    {
        try
        {
            Log.Information("Fetching books with parameters: {@Parameters}", parameters);

            var xmlParams = Common.CommonHelper.DictionaryToXml(parameters, "Search");
            string query = "sp_DynamicGetAllBooks {0}";
            object[] param = { xmlParams };

            var res = await _spContext.ExecutreStoreProcedureResultList(query, param);
            List<BookResponseModel> list =
                JsonConvert.DeserializeObject<List<BookResponseModel>>(res.Result?.ToString() ?? "[]") ?? [];

            Log.Information("Fetched {Count} books from DB.", list?.Count ?? 0);

            return list ?? new List<BookResponseModel>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in List method.");
            throw new HttpStatusCodeException(500, ex.Message);
        }
    }

    public async Task<BookResponseModel> GetBookBySid(string bookSid)
    {
        try
        {
            Log.Information("Fetching book by SID: {BookSid}", bookSid);
            string query = "sp_getBookBySid {0}";
            object[] param = { bookSid };
            var student = await _spContext.ExecuteStoreProcedure(query, param);
            BookResponseModel book = JsonConvert.DeserializeObject<BookResponseModel>(student?.ToString() ?? "{}");

            if (book == null)
            {
                Log.Warning("No book found with SID: {BookSid}", bookSid);
                throw new HttpStatusCodeException(400, "No book found with SID: {BookSid}");
                return new BookResponseModel();
            }

            Log.Information("Book found: {@Book}", book);
            return book;
        }
        catch (HttpStatusCodeException ex)
        {
            Log.Warning("No book found with SID: {BookSid}", bookSid);
            throw new HttpStatusCodeException(400, "No book found with SID: {BookSid}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in GetBookBySid for SID {BookSid}", bookSid);
            throw new HttpStatusCodeException(500, ex.Message);
        }
    }

    public async Task<BookResponseModel> CreateAsync(BookRequestModel book)
    {
        try
        {
            Log.Information("Creating new book: {@Book}", book);

            Book b = new Book
            {
                BookSid = "BSID" + Guid.NewGuid().ToString(),
                Author = book.Author,
                Title = book.Title,
                Isbn = book.Isbn,
                PublishedYear = book.PublishedYear,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsAvailable = (int)StatusEnum.Active
            };

            await _unitOfWork.GetRepository<Book>().InsertAsync(b);
            await _unitOfWork.CommitAsync();

            var bookres = new BookResponseModel
            {
                BookSid = b.BookSid,
                Author = b.Author,
                Title = b.Title,
                Isbn = b.Isbn,
                PublishedYear = b.PublishedYear,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                IsAvailable = b.IsAvailable
            };

            Log.Information("Book created successfully with SID: {BookSid}", b.BookSid);
            return bookres;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while creating book: {@Book}", book);
            throw new HttpStatusCodeException(500, ex.Message);
        }
    }

    public async Task<List<BookResponseModel>> CreateAsyncMul(List<BookRequestModel> books)
    {
        try
        {
            Log.Information("Creating multiple books. Count: {Count}", books.Count);

            List<Book> bookList = new();
            foreach (BookRequestModel book in books)
            {
                Book b = new Book
                {
                    BookSid = "BSID" + Guid.NewGuid().ToString(),
                    Author = book.Author,
                    Title = book.Title,
                    PublishedYear = book.PublishedYear,
                    Isbn = book.Isbn,
                    IsAvailable = (int)StatusEnum.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                bookList.Add(b);
            }

            await _unitOfWork.GetRepository<Book>().InsertAsync(bookList);
            await _unitOfWork.CommitAsync();

            List<BookResponseModel> resBooks = bookList.Select(b => new BookResponseModel
            {
                BookSid = b.BookSid,
                Author = b.Author,
                Title = b.Title,
                PublishedYear = b.PublishedYear,
                Isbn = b.Isbn,
                IsAvailable = b.IsAvailable,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToList();

            Log.Information("Successfully created {Count} books.", resBooks.Count);
            return resBooks;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while creating multiple books.");
            throw new HttpStatusCodeException(500, ex.Message);
        }
    }

    public async Task<bool> UpdateAsync(string booksid, BookRequestModel book)
    {
        try
        {
            Log.Information("Updating book with SID: {BookSid}", booksid);

            var b = await _unitOfWork.GetRepository<Book>()
                .SingleOrDefaultAsync(x => x.BookSid == booksid);
            if (b == null)
            {
                Log.Warning("Book not found with SID: {BookSid}", booksid);
                throw new HttpStatusCodeException(400, "Book not found");
            }

            b.Title = book.Title;
            b.UpdatedAt = DateTime.UtcNow;
            b.Author = book.Author;
            b.Isbn = book.Isbn;
            b.PublishedYear = book.PublishedYear;

            _unitOfWork.GetRepository<Book>().Update(b);
            await _unitOfWork.CommitAsync();

            Log.Information("Book with SID {BookSid} updated successfully.", booksid);
            return true;
        }
        catch (HttpStatusCodeException ex)
        {
            Log.Warning("Update failed. Book not found with SID: {BookSid}", booksid);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while updating book with SID: {BookSid}", booksid);
            throw new HttpStatusCodeException(400, "Bad request");
        }
    }

    public async Task<bool> DeleteAsync(string booksid)
    {
        try
        {
            Log.Information("Deleting book with SID or Title: {BookSid}", booksid);

            var b = await _unitOfWork.GetRepository<Book>()
                .SingleOrDefaultAsync(x =>
                    (x.Status != (int)StatusEnum.Deleted) && (x.BookSid == booksid || x.Title == booksid));

            if (b == null)
            {
                Log.Warning("Book not found for deletion. SID/Title: {BookSid}", booksid);
                throw new HttpStatusCodeException(400, "Book not found");
            }

            b.Status = (int)StatusEnum.Deleted;
            _unitOfWork.GetRepository<Book>().Update(b);
            await _unitOfWork.CommitAsync();

            Log.Information("Book with SID {BookSid} marked as deleted.", booksid);
            return true;
        }
        catch (HttpStatusCodeException ex)
        {
            Log.Warning("Delete failed for book {BookSid}. Reason: {Message}", booksid, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Internal error while deleting book with SID: {BookSid}", booksid);
            throw new HttpStatusCodeException(500, "Internal server error");
        }
    }

    public async Task<bool> UpdateStatus(string booksid, string isbn,int status)
    {
        try
        {
            var book = await _unitOfWork.GetRepository<Book>()
                .SingleOrDefaultAsync(x =>
                    (x.IsAvailable != (int)StatusEnum.Deleted) && (x.BookSid == booksid && x.Isbn == isbn) && (x.IsAvailable!=status));
            if (book == null)
            {
                Log.Warning("Book not found with SID: {BookSid}", booksid);
                throw new HttpStatusCodeException(400, "Book not found");
            }

            book.IsAvailable = status;
            _unitOfWork.GetRepository<Book>().Update(book);
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (HttpStatusCodeException ex)
        {
            throw new HttpStatusCodeException(400, "Book not found");
        }
        catch (Exception ex)
        {
            throw new HttpStatusCodeException(500, "Internal server error");
        }
    }
}
