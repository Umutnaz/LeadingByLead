using Backend.Repositories;
using Backend.Services;
using DotNetEnv;
using MongoDB.Driver;
using Microsoft.AspNetCore.StaticFiles;
var builder = WebApplication.CreateBuilder(args);

// Load .env when it exists.
// Locally this reads LeadingByLead/.env.
// In Docker/Render the Dockerfile copies .env into the container.
if (File.Exists(".env"))
{
    Env.Load();
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var environmentConnectionString =
        Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");

    var connectionString =
        !string.IsNullOrWhiteSpace(environmentConnectionString)
            ? environmentConnectionString
            : configuration.GetValue<string>("MongoDbSettings:ConnectionString");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        connectionString = "mongodb://localhost:27017";
    }

    return new MongoClient(connectionString);
});

builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var client = serviceProvider.GetRequiredService<IMongoClient>();

    var environmentDatabaseName =
        Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");

    var databaseName =
        !string.IsNullOrWhiteSpace(environmentDatabaseName)
            ? environmentDatabaseName
            : configuration.GetValue<string>("MongoDbSettings:DatabaseName")
              ?? "LeadingByLeadDb";

    return client.GetDatabase(databaseName);
});

builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();

var app = builder.Build();

var renderPort = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(renderPort))
{
    app.Urls.Add($"http://0.0.0.0:{renderPort}");
}

var database = app.Services.GetRequiredService<IMongoDatabase>();
var existingCollections = await database.ListCollectionNames().ToListAsync();

string[] requiredCollections =
{
    "Characters",
    "Questions",
    "GameSessions"
};

foreach (var collectionName in requiredCollections)
{
    if (!existingCollections.Contains(collectionName))
    {
        Console.WriteLine($"Creating missing collection: {collectionName}");
        await database.CreateCollectionAsync(collectionName);
    }
}

var seedEnvironmentValue =
    Environment.GetEnvironmentVariable("SEED_STATIC_DATA");

var shouldSeed =
    string.Equals(
        seedEnvironmentValue,
        "true",
        StringComparison.OrdinalIgnoreCase);

if (seedEnvironmentValue is null && app.Environment.IsDevelopment())
{
    shouldSeed = true;
}

if (shouldSeed)
{
    Console.WriteLine("Running SeedData.SeedAsync...");
    await SeedData.SeedAsync(app.Services);
}
else
{
    Console.WriteLine("Skipping static seed data.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();

var contentTypeProvider = new FileExtensionContentTypeProvider();

contentTypeProvider.Mappings[".dat"] = "application/octet-stream";
contentTypeProvider.Mappings[".dll"] = "application/octet-stream";
contentTypeProvider.Mappings[".wasm"] = "application/wasm";
contentTypeProvider.Mappings[".pdb"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "healthy",
        application = "LeadingByLead"
    }));

app.MapFallbackToFile("index.html");

app.Run();
