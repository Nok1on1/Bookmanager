using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Learning.Models;


/// <summary>
/// Model of the book
/// </summary>
public class Book
{
    /// <summary>
    /// ObjectId of the given book in MongoDb
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [SwaggerIgnore]
    public string? Id { get; set; }

    /// <summary>
    /// Primary Key
    /// </summary>
    public required string  Title { get; set; }
    /// <summary>
    /// Publication Year
    /// </summary>
    public int PublicationYear { get; set; }
    /// <summary>
    /// Author Name
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Number of times book has been seen
    /// increments on /GetBook
    /// </summary>
    public int ViewCount {get; set;}

    /// <summary>
    /// True if soft deleted
    /// </summary>
    [BsonElement("isDeleted")]
    [SwaggerIgnore]
    public bool IsDeleted { get; set; }
}
