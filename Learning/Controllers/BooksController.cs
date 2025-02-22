using Learning.Models;
using Learning.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace Learning.Controllers;

/// <summary>
/// CRUD Operations For Books
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BooksService _booksService;

    public BooksController(BooksService booksService) => _booksService = booksService;

    /// <summary>
    /// Returns all the Books
    /// </summary>
    [HttpGet("/getEveryBook", Name = "GetEveryBook")]
    public async Task<List<string>> Get(int pageNum, int pageSize)
    {
        var books = await _booksService.GetByPopularityAsync(pageNum, pageSize);
        return books.Select(b => b.Title).ToList();
    }



    /// <summary>
    /// Return Book By its Title
    /// </summary>
    [HttpGet("/GetBook", Name = "GetBook")]
    public async Task<ActionResult<Book>> Get(string title)
    {
        var book = await _booksService.GetAsync(title);

        if (book is null || book.IsDeleted)
        {
            return NotFound();
        }

        await _booksService.UpdateViewCountAsync(title);

        return Ok(book);
    }

    /// <summary>
    /// Insert New Books in DB
    /// </summary>
    [HttpPost("/addBooks", Name = "AddBooks")]
    public async Task<IActionResult> Post(Book[] newBooks)
    {
        // Adding SoftDeleted Book will give us an error.
        // it will also be Not very convenient to not give an error U did soft delete for what purpose??!?!!??!?!
        try
        {
            await _booksService.CreateAsync(newBooks);
        }
        catch (MongoBulkWriteException<Book> e) when (e.WriteErrors.Any(error =>
                                                          error.Category == ServerErrorCategory.DuplicateKey))
        {
            // Get Index on Which Books CreateAsync Fails
            var failedIndexes = e.WriteErrors.Select(error => error.Index).ToList();
            // Title Of the Failed Books
            var failedBooks = failedIndexes.Select(index => newBooks[index].Title).ToList();

            return new BadRequestObjectResult("Duplicate key(s) on: " + string.Join(", ", failedBooks));
        }

        return Accepted();
    }

    /// <summary>
    /// Updates Book in database
    /// </summary>
    [HttpPut("/updateBook", Name = "UpdateBook")]
    public async Task<IActionResult> Update(string title, Book updatedBook)
    {
        var result = await _booksService.UpdateAsync(title, updatedBook);

        return result.MatchedCount != 0 ? NoContent() : NotFound($"No book found with title \"{title}\".");
    }

    /// <summary>
    /// Soft Delete Books
    /// </summary>
    [HttpDelete("/softDeleteBooks", Name = "SoftDeleteBooks")]
    public async Task<IActionResult> SoftDelete(string[] titles)
    {
        List<string> savedBooks = new();

        foreach (var title in titles)
        {
         var result = await _booksService.SoftDeleteAsync(title);
         if (result.MatchedCount == 0)
             savedBooks.Add(title);
        }

        return savedBooks.IsNullOrEmpty() ? NoContent() : NotFound("No books found with title:"
                                                                   + string.Join(", ", savedBooks));
    }
}