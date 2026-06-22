using DotNetEnv;
using Backend.Repositories;
using Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Load .env file (if present) so Environment.GetEnvironmentVariable can read values from it
Env.Load();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Allow the frontend origin(s) - adjust ports if different
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

// Configure MongoDB - prefer environment variables (from .env or environment) but fall back to appsettings.json
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var connFromEnv = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
    var conn = !string.IsNullOrWhiteSpace(connFromEnv)
        ? connFromEnv
        : cfg.GetValue<string>("MongoDbSettings:ConnectionString");

    if (string.IsNullOrWhiteSpace(conn))
    {
        // Default to localhost for developer convenience
        conn = "mongodb://localhost:27017";
    }

    return new MongoClient(conn);
});

builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var client = sp.GetRequiredService<IMongoClient>();
    var dbFromEnv = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");
    var dbName = !string.IsNullOrWhiteSpace(dbFromEnv) ? dbFromEnv : cfg.GetValue<string>("MongoDbSettings:DatabaseName") ?? "LeadingByLeadDb";
    return client.GetDatabase(dbName);
});

// Register repositories
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();

var app = builder.Build();

// Seed static data on startup if requested via SEED_STATIC_DATA=true in environment/.env
var seedStaticDataEnv = Environment.GetEnvironmentVariable("SEED_STATIC_DATA");
// Default to true in Development if not explicitly set to speed up local testing
var seedStaticData = seedStaticDataEnv is not null && seedStaticDataEnv.Trim().ToLowerInvariant() == "true";
if (seedStaticDataEnv is null && app.Environment.IsDevelopment()) seedStaticData = true;

// Ensure database and collections exist (creates when missing)
var database = app.Services.GetRequiredService<IMongoDatabase>();
var existingCollections = await database.ListCollectionNames().ToListAsync();
string[] required = new[] { "Characters", "Questions", "GameSessions" };
foreach (var name in required)
{
    if (!existingCollections.Contains(name))
    {
        Console.WriteLine($"Creating missing collection: {name}");
        await database.CreateCollectionAsync(name);
    }
}

if (seedStaticData)
{
    Console.WriteLine("SEED_STATIC_DATA=true or Development -> running SeedData.SeedAsync...");
    await SeedData.SeedAsync(app.Services);
}
else
{
    Console.WriteLine("SEED_STATIC_DATA not set or false -> skipping seed data on startup.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS before controllers
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();


app.Run();