using Learning.Models;
using Learning.Services;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<BooksService>();

// Add Controllers
builder.Services.AddControllers();

// Swagger Configuration
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Books API",
        Version = "v1",
        Description = "RESTful API using ASP.NET Core Web API that allows managing books.",
        Contact = new OpenApiContact
        {
            Name = "Giorgi",
            Email = "mikaberidze.giorgi@kiu.edu.ge"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Books API V1");
    });

    app.MapOpenApi();
}

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

app.Run();