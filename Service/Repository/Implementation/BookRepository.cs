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
            var xmlParams = Common.CommonHelper.DictionaryToXml(parameters, "Search");
            string query = "sp_DynamicGetAllBooks {0}";
            object[] param = { xmlParams };
            var res = await _spContext.ExecutreStoreProcedureResultList(query, param);
            List<BookResponseModel> list =
                JsonConvert.DeserializeObject<List<BookResponseModel>>(res.Result?.ToString() ?? "[]") ?? [];
            if (list != null)
            {
                return list;
            }
            return new List<BookResponseModel>();  
        }
        
        catch (Exception ex)
        {
            Console.WriteLine($"Error in List: {ex.Message}");
            throw new HttpStatusCodeException(500,ex.Message);    
        }
    }

    public async Task<BookResponseModel> GetBookBySid(string bookSid)
    {
        try
        {
            string query = "sp_getBookBySid {0}";
            object[] param = { bookSid };
            var student = await _spContext.ExecuteStoreProcedure(query, param);
            BookResponseModel book = Newtonsoft.Json.JsonConvert.DeserializeObject<BookResponseModel>(student?.ToString() ?? "{}");
            if (book != null)
            {
                return book;
            }
            return new BookResponseModel();
        }catch(Exception ex)
        {
            Console.WriteLine($"Error in GetBookBySid: {ex.Message}");
            throw new HttpStatusCodeException(500, ex.Message);
        }
    }

    public async Task<BookResponseModel> CreateAsync(BookRequestModel book)
    {
        try
        {
            Book b = new Book();
            b.BookSid = "BSID" + Guid.NewGuid().ToString();
            b.Author =  book.Author;
            b.Title = book.Title;
            b.Isbn =  book.Isbn;
            b.PublishedYear = book.PublishedYear;
            b.CreatedAt = DateTime.UtcNow;
            b.UpdatedAt = DateTime.UtcNow;
            b.IsAvailable = (int)StatusEnum.Active;
            await _unitOfWork.GetRepository<Book>().InsertAsync(b);
            await _unitOfWork.CommitAsync();
            BookResponseModel bookres = new BookResponseModel();
            bookres.BookSid = b.BookSid;
            bookres.Author = b.Author;
            bookres.Title = b.Title;
            bookres.Isbn = book.Isbn;
            bookres.PublishedYear = b.PublishedYear;
            bookres.CreatedAt = b.CreatedAt;
            bookres.UpdatedAt = b.UpdatedAt;
            bookres.IsAvailable = b.IsAvailable;
            return bookres;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateAsync: {ex.Message}");
            throw new HttpStatusCodeException(500, ex.Message);
        }
    }

    public async  Task<List<BookResponseModel>> CreateAsyncMul(List<BookRequestModel> books)
    {
        try
        {
            List<Book> bookList = new List<Book>();
            foreach (BookRequestModel book in books)
            {
                Book b = new Book();
                b.BookSid = "BSID" + Guid.NewGuid().ToString();
                b.Author = book.Author;
                b.Title = book.Title;
                b.PublishedYear = book.PublishedYear;
                b.Isbn = book.Isbn;
                b.IsAvailable = (int)StatusEnum.Active;
                b.CreatedAt = DateTime.UtcNow;
                b.UpdatedAt = DateTime.UtcNow;
                Console.WriteLine("===============================");
                Console.WriteLine(b.BookSid);
                Console.WriteLine(b.Author);
                Console.WriteLine(b.Title);
                Console.WriteLine(b.Isbn);
                Console.WriteLine(b.PublishedYear);
                Console.WriteLine(b.IsAvailable);
                Console.WriteLine(b.CreatedAt);
                Console.WriteLine(b.UpdatedAt);
                Console.WriteLine(b.Isbn);
                Console.WriteLine("===============================");

                bookList.Add(b);
                
            }
            await _unitOfWork.GetRepository<Book>().InsertAsync(bookList);
            await _unitOfWork.CommitAsync();
            
            List<BookResponseModel> resBooks = new List<BookResponseModel>();
            foreach (Book b in bookList)
            {
                BookResponseModel resBook = new BookResponseModel();
                resBook.BookSid = b.BookSid;
                resBook.Author = b.Author;
                resBook.Title = b.Title;
                resBook.PublishedYear = b.PublishedYear;
                resBook.Isbn = b.Isbn;
                resBook.IsAvailable = b.IsAvailable;
                resBook.CreatedAt = b.CreatedAt;
                resBook.UpdatedAt = b.UpdatedAt;
                resBooks.Add(resBook);
            }
    
            return resBooks;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateAsync: {ex.Message}");
            throw new HttpStatusCodeException(500, ex.Message);
        }
    }
    public async Task<bool> UpdateAsync(string booksid,BookRequestModel book)
    {
        try
        {
            var b = await _unitOfWork.GetRepository<Book>()
                .SingleOrDefaultAsync(x => x.BookSid == booksid);
            if (b == null)
            {
                return false;
            }

            b.Title = book.Title;
            b.UpdatedAt = DateTime.UtcNow;
            b.Author = book.Author;
            b.Isbn = book.Isbn;
            b.PublishedYear = book.PublishedYear;
            _unitOfWork.GetRepository<Book>().Update(b);
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateAsync: {ex.Message}");
            throw new HttpStatusCodeException(500, ex.Message);
        }
    }

    public async Task<bool> DeleteAsync(string booksid)
    {
        try
        {
            var b = await _unitOfWork.GetRepository<Book>()
                .SingleOrDefaultAsync( x =>(x.IsAvailable !=(int) StatusEnum.Deleted) &&x.BookSid == booksid || x.Title == booksid);
            if (b == null)
            {
                return false;
            }
            b.IsAvailable = (int)StatusEnum.Deleted;
            _unitOfWork.GetRepository<Book>().Update(b);
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DeleteAsync: {ex.Message}");
            throw new HttpStatusCodeException(500, ex.Message);
        }
    }
}