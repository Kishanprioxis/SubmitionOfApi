using Models.CommonModel;
using Models.RequestModel;
using Models.ResponseModel;

namespace Service.Repository.Interface;

public interface IBookRepository
{
    Task<Page> List(Dictionary<string, object> parameters);
    Task<BookResponseModel> GetBookBySid(string bookSid);
    Task<BookResponseModel> CreateAsync(BookRequestModel book);
    Task<bool> UpdateAsync(string booksid, BookRequestModel book);
    Task<bool> DeleteAsync(string booksid);

    Task<List<BookResponseModel>> CreateAsyncMul(List<BookRequestModel> books);
    Task<bool> UpdateStatus(string booksid,string isbn,int status);
    //Task<List<BookResponseModel>> CreateAsync(List<BookRequestModel> book);
}