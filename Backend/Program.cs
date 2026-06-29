using Backend.Repositories;
using Backend.Services;
using DotNetEnv;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Brug kun lokal .env-fil, hvis den faktisk eksisterer.
// På Render bruges environment variables fra dashboardet.
if (File.Exists(".env"))
{
    Env.Load();
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS bruges primært under lokal udvikling,
// hvor frontend og backend kører på forskellige porte.
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

// MongoDB
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var configuration =
        serviceProvider.GetRequiredService<IConfiguration>();

    var environmentConnectionString =
        Environment.GetEnvironmentVariable(
            "MONGO_CONNECTION_STRING");

    var connectionString =
        !string.IsNullOrWhiteSpace(environmentConnectionString)
            ? environmentConnectionString
            : configuration.GetValue<string>(
                "MongoDbSettings:ConnectionString");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        connectionString = "mongodb://localhost:27017";
    }

    return new MongoClient(connectionString);
});

builder.Services.AddSingleton(serviceProvider =>
{
    var configuration =
        serviceProvider.GetRequiredService<IConfiguration>();

    var client =
        serviceProvider.GetRequiredService<IMongoClient>();

    var environmentDatabaseName =
        Environment.GetEnvironmentVariable(
            "MONGO_DATABASE_NAME");

    var databaseName =
        !string.IsNullOrWhiteSpace(environmentDatabaseName)
            ? environmentDatabaseName
            : configuration.GetValue<string>(
                "MongoDbSettings:DatabaseName")
              ?? "LeadingByLeadDb";

    return client.GetDatabase(databaseName);
});

// Repositories
builder.Services.AddScoped<
    ICharacterRepository,
    CharacterRepository>();

builder.Services.AddScoped<
    IQuestionRepository,
    QuestionRepository>();

builder.Services.AddScoped<
    IGameSessionRepository,
    GameSessionRepository>();

var app = builder.Build();

// Render sender porten gennem environment variable PORT.
// Render kræver, at applikationen lytter på 0.0.0.0.
var renderPort =
    Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(renderPort))
{
    app.Urls.Add($"http://0.0.0.0:{renderPort}");
}

// Opret MongoDB collections, hvis de mangler.
var database =
    app.Services.GetRequiredService<IMongoDatabase>();

var existingCollections =
    await database
        .ListCollectionNames()
        .ToListAsync();

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
        Console.WriteLine(
            $"Creating missing collection: {collectionName}");

        await database.CreateCollectionAsync(
            collectionName);
    }
}

// Seed-data
var seedEnvironmentValue =
    Environment.GetEnvironmentVariable(
        "SEED_STATIC_DATA");

var shouldSeed =
    string.Equals(
        seedEnvironmentValue,
        "true",
        StringComparison.OrdinalIgnoreCase);

// Lokalt seeds databasen automatisk,
// medmindre SEED_STATIC_DATA specifikt er sat.
if (seedEnvironmentValue == null &&
    app.Environment.IsDevelopment())
{
    shouldSeed = true;
}

if (shouldSeed)
{
    Console.WriteLine(
        "Running SeedData.SeedAsync...");

    await SeedData.SeedAsync(app.Services);
}
else
{
    Console.WriteLine(
        "Skipping static seed data.");
}

// Swagger kun lokalt
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Frontendfiler fra Backend/wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();

// Render health check
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        application = "LeadingByLead"
    });
});

// Alle Blazor-ruter sendes til index.html.
// Det gør fx /gameeditor og /waiting-area mulige ved refresh.
app.MapFallbackToFile("index.html");

app.Run();