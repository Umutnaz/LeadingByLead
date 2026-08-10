using Backend.Repositories;
using Core;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Services;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var characterRepository =
            scope.ServiceProvider.GetRequiredService<ICharacterRepository>();

        var questionRepository =
            scope.ServiceProvider.GetRequiredService<IQuestionRepository>();

        var characters = await characterRepository.GetAllAsync();

        if (characters.Count == 0)
        {
            characters = CreateCharacters();

            foreach (var character in characters)
            {
                character.ResetCurrentStats();
                await characterRepository.CreateAsync(character);
            }
        }

        var questions = await questionRepository.GetAllAsync();

        if (questions.Count == 0)
        {
            foreach (var question in CreateQuestions(characters))
                await questionRepository.CreateAsync(question);
        }
    }

    private static List<Character> CreateCharacters()
    {
        return new List<Character>
        {
            new()
            {
                Name = "Daniel - GV1",
                Description = "",
                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 82,
                    Sociallyst = 45,
                    Tillid = 66,
                    Stress = 32
                }
            },
            new()
            {
                Name = "Charlie - LMG1",
                Description = "",
                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 78,
                    Sociallyst = 38,
                    Tillid = 64,
                    Stress = 30
                }
            },
            new()
            {
                Name = "Inzo - GV3",
                Description = "",
                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 58,
                    Sociallyst = 88,
                    Tillid = 72,
                    Stress = 25
                }
            },
            new()
            {
                Name = "Søren - LMG2",
                Description = "",
                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 66,
                    Sociallyst = 52,
                    Tillid = 64,
                    Stress = 40
                }
            },
            new()
            {
                Name = "Ida-Sofie - GV2",
                Description = "",
                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 70,
                    Sociallyst = 80,
                    Tillid = 64,
                    Stress = 34
                }
            },
            new()
            {
                Name = "Lene - GV4",
                Description = "",
                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 62,
                    Sociallyst = 74,
                    Tillid = 70,
                    Stress = 38
                }
            },
        };
    }