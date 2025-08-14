using Microsoft.AspNetCore.Mvc;
using Models.CommonModel;
using Models.RequestModel;
using Models.ResponseModel;
using Service.Repository.Interface;
using Serilog; 

namespace DemoProject.Controllers
{
    public class BookController : BaseController
    {
        private readonly IBookRepository _bookRepository;

        public BookController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet("getAllBooks")]
        public async Task<ActionResult<List<IBookRepository>>> GetAllBooks([FromQuery] SearchRequestModel searchRequestModel)
        {
            try
            {
                var parameters = FillParamesFromModel(searchRequestModel);
                var list = await _bookRepository.List(parameters);

                if (list == null)
                {
                    Log.Warning("No books found for search: {@SearchParams}", searchRequestModel);
                    return NotFound();
                }

                Log.Information("Fetched {Count} books successfully.", list.Count);
                return Ok(list);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while fetching all books.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("getBookBySId/{BookSID}")]
        public async Task<ActionResult<BookResponseModel>> GetBookById([FromRoute] string BookSID)
        {
            try
            {
                var book = await _bookRepository.GetBookBySid(BookSID);
                if (book == null)
                {
                    Log.Warning("Book with SID {BookSID} not found.", BookSID);
                    return NotFound();
                }

                Log.Information("Fetched book with SID {BookSID} successfully.", BookSID);
                return Ok(book);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while fetching book with SID {BookSID}", BookSID);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("createBook")]
        public async Task<ActionResult<BookResponseModel>> CreateAsync([FromBody]BookRequestModel book)
        {
            try
            {
                var createdBook = await _bookRepository.CreateAsync(book);
                if (createdBook == null)
                {
                    Log.Warning("Failed to create book: {@BookData}", book);
                    return BadRequest();
                }

                Log.Information("Book created successfully with SID:");
                return Ok(createdBook);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while creating book: {@BookData}", book);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("createBookMultiple")]
        public async Task<ActionResult<List<BookResponseModel>>> CreateAsync([FromBody] List<BookRequestModel> book)
        {
            try
            {
                 List<BookResponseModel> createdBook = await _bookRepository.CreateAsyncMul(book);
                if (createdBook == null)
                {
                    Log.Warning("Failed to create book: {@BookData}", book);
                    return BadRequest();
                }

                Log.Information("Book created successfully with SID:");
                return Ok(createdBook);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while creating book: {@BookData}", book);
                return StatusCode(500, "Internal server error.");
            }
        }
    
        [HttpPost("updateBook/{BookSID}")]
        public async Task<ActionResult<BookResponseModel>> Update([FromBody] BookRequestModel book, [FromRoute] string BookSID)
        {
            try
            {
                var success = await _bookRepository.UpdateAsync(BookSID, book);
                if (success)
                {
                    Log.Information("Book with SID {BookSID} updated successfully.", BookSID);
                    return Ok(success);
                }

                Log.Warning("Book with SID {BookSID} not found for update.", BookSID);
                return NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while updating book with SID {BookSID}", BookSID);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("deleteBook/{BookSID}")]
        public async Task<ActionResult<BookResponseModel>> Delete([FromRoute] string BookSID)
        {
            try
            {
                var success = await _bookRepository.DeleteAsync(BookSID);
                if (success)
                {
                    Log.Information("Book with SID {BookSID} deleted successfully.", BookSID);
                    return Ok(success);
                }

                Log.Warning("Book with SID {BookSID} not found for deletion.", BookSID);
                return NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while deleting book with SID {BookSID}", BookSID);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
