using Learning.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Learning.Services;

/// <summary>
/// I/O Operations for MongoDb
/// </summary>
public class BooksService
{
    // ref to Database
    private readonly IMongoCollection<Book> _booksCollection;


    public BooksService(IOptions<MongoDbSettings> mongoDbSettings)
    {
        // Assign Collection To _booksCollection
        MongoClient client = new MongoClient(mongoDbSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _booksCollection = database.GetCollection<Book>(mongoDbSettings.Value.CollectionName);

        // Make Title Primary Key
        // Creates IndexKey on Title. Ascending Helps Speed Up A -> Z Query
        var indexKeys = Builders<Book>.IndexKeys.Ascending(x => x.Title);
        // Option Key To be Unique
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<Book>(indexKeys, indexOptions);
        _booksCollection.Indexes.CreateOneAsync(indexModel);
    }


    /// <summary>
    /// Returns Every Book On given Page Sorted By Popularity
    /// </summary>
    public async Task<List<Book>> GetByPopularityAsync(int pageNum, int pageSize) =>
        await _booksCollection.Aggregate()
            .Match(book => !book.IsDeleted) //Check if Soft Deleted
            .Project(book => new
            {
                book,
                Popularity = book.ViewCount * 0.5 + (DateTime.Today.Year - book.PublicationYear) * 2
            }) // Assign Popularity To Each Book
            .SortByDescending(b => b.Popularity) //Sort by popularity
            .Skip((pageNum - 1) * pageSize) // Keeps Books Only After the Given (page - 1)
            .Limit(pageSize) // Finally Keeps Books Only From the given page
            .Project(bp => bp.book) // Drop The popularity
            .ToListAsync();

    /// <summary>
    /// return Book by its title
    /// </summary>
    public async Task<Book?> GetAsync(string title) =>
        await _booksCollection.Find(x => x.Title == title).FirstOrDefaultAsync();

    /// <summary>
    /// Inserts new book(s) in Database
    /// </summary>
    public async Task CreateAsync(IEnumerable<Book> books) =>
        // If One of the Books Fail It Will Still Insert Other Ones
        await _booksCollection.InsertManyAsync(books, new InsertManyOptions { IsOrdered = false });

    /// <summary>
    /// updates The book
    /// </summary>
    public async Task<ReplaceOneResult> UpdateAsync(string title, Book updatedBook)
    {
        var book = await _booksCollection.Find(x => x.Title == title).FirstOrDefaultAsync();

        if (book is null || book.IsDeleted) //Matched count = 0;
            return new ReplaceOneResult.Acknowledged(0,0, null);

        updatedBook.Id = book.Id;
    return await _booksCollection.ReplaceOneAsync(x => x.Title == title, updatedBook);
    }

/// <summary>
    /// Updates ViewCount of the given book in async (hopefully)
    /// </summary>
    public async Task UpdateViewCountAsync(string title) =>
        await _booksCollection.UpdateOneAsync(x => 
            x.Title == title, Builders<Book>.Update.Inc("ViewCount", 1));


    /// <summary>
    /// SoftDelete
    /// </summary>
    public async Task<UpdateResult> SoftDeleteAsync(string title) =>
        await _booksCollection.UpdateOneAsync(x =>
            x.Title == title, Builders<Book>.Update.Set("isDeleted", true));
}