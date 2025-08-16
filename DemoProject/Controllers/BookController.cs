using Microsoft.AspNetCore.Mvc;
using Models.CommonModel;
using Models.RequestModel;
using Models.ResponseModel;
using Service.Repository.Interface;
using Serilog; 

namespace DemoProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : BaseController
    {
        private readonly IBookRepository _bookRepository;

        public BookController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet("getAllBooks")]
        public async Task<ActionResult<IEnumerable<BookResponseModel>>> GetAllBooks([FromQuery] SearchRequestModel searchRequestModel)
        {
            var parameters = FillParamesFromModel(searchRequestModel);
            var list = await _bookRepository.List(parameters);

            if (list == null || !list.Any())
            {
                Log.Warning("No books found for search: {@SearchParams}", searchRequestModel);
                return NotFound();
            }

            Log.Information("Fetched {Count} books successfully.", list.Count);
            return Ok(list);
        }

        [HttpGet("getBookBySId/{BookSID}")]
        public async Task<ActionResult<BookResponseModel>> GetBookById([FromRoute] string BookSID)
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

        [HttpPost("createBook")]
        public async Task<ActionResult<BookResponseModel>> CreateAsync([FromBody] BookRequestModel book)
        {
            var createdBook = await _bookRepository.CreateAsync(book);
            if (createdBook == null)
            {
                Log.Warning("Failed to create book: {@BookData}", book);
                return BadRequest();
            }

            Log.Information("Book created successfully with SID {BookSID}", createdBook.BookSid);
            return CreatedAtAction(nameof(GetBookById), new { BookSID = createdBook.BookSid }, createdBook);
        }

        [HttpPost("createBookMultiple")]
        public async Task<ActionResult<IEnumerable<BookResponseModel>>> CreateAsync([FromBody] List<BookRequestModel> books)
        {
            var createdBooks = await _bookRepository.CreateAsyncMul(books);
            if (createdBooks == null || !createdBooks.Any())
            {
                Log.Warning("Failed to create multiple books: {@BookData}", books);
                return BadRequest();
            }

            Log.Information("Multiple books created successfully. Count: {Count}", createdBooks.Count);
            return Ok(createdBooks);
        }

        [HttpPost("updateBook/{BookSID}")]
        public async Task<IActionResult> Update([FromBody] BookRequestModel book, [FromRoute] string BookSID)
        {
            var success = await _bookRepository.UpdateAsync(BookSID, book);
            if (!success)
            {
                Log.Warning("Book with SID {BookSID} not found for update.", BookSID);
                return NotFound();
            }

            Log.Information("Book with SID {BookSID} updated successfully.", BookSID);
            return NoContent();
        }

        [HttpDelete("deleteBook/{BookSID}")]
        public async Task<IActionResult> Delete([FromRoute] string BookSID)
        {
            var success = await _bookRepository.DeleteAsync(BookSID);
            if (!success)
            {
                Log.Warning("Book with SID {BookSID} not found for deletion.", BookSID);
                return NotFound();
            }

            Log.Information("Book with SID {BookSID} deleted successfully.", BookSID);
            return NoContent();
        }
    }
}
