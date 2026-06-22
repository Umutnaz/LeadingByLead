using Core;
using Backend.Repositories;
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
            var seededQuestions = CreateQuestions(characters);

            foreach (var question in seededQuestions)
                await questionRepository.CreateAsync(question);
        }
    }

    private static List<Character> CreateCharacters()
    {
        return new List<Character>
        {
            new()
            {
                Name = "Sergent Jensen",
                Description =
                    "Erfaren og rolig leder, som normalt står stærkt under pres.",

                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 85,
                    Stress = 25,
                    Sociallyst = 55,
                    Tillid = 80
                }
            },

            new()
            {
                Name = "Korporal Hansen",
                Description =
                    "Ung og energisk soldat, som søger fællesskab og handling.",

                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 90,
                    Stress = 45,
                    Sociallyst = 85,
                    Tillid = 60
                }
            },

            new()
            {
                Name = "Specialist Larsen",
                Description =
                    "Teknisk dygtig specialist, som fungerer bedst med tydelige rammer.",

                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 75,
                    Stress = 55,
                    Sociallyst = 30,
                    Tillid = 70
                }
            }
        };
    }

    private static List<Question> CreateQuestions(
        List<Character> characters)
    {
        var jensen = characters.First(
            character => character.Name == "Sergent Jensen");

        var hansen = characters.First(
            character => character.Name == "Korporal Hansen");

        var larsen = characters.First(
            character => character.Name == "Specialist Larsen");

        return new List<Question>
        {
            new()
            {
                Text =
                    "Gruppen har været på øvelse i mange timer. Flere virker trætte, men missionen er ikke færdig. Hvad gør du?",

                AnswerOptions = new List<AnswerOption>
                {
                    new()
                    {
                        Text = "Giv gruppen en kort pause",

                        CharacterEffects = new List<CharacterEffect>
                        {
                            Effect(
                                jensen,
                                Change(nameof(CurrentStats.Stress), -10),
                                Change(nameof(CurrentStats.Tillid), 5)),

                            Effect(
                                hansen,
                                Change(nameof(CurrentStats.Stress), -20),
                                Change(nameof(CurrentStats.Sociallyst), 10),
                                Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                            Effect(
                                larsen,
                                Change(nameof(CurrentStats.Stress), -25),
                                Change(nameof(CurrentStats.Tillid), 10))
                        }
                    },

                    new()
                    {
                        Text = "Pres gruppen videre",

                        CharacterEffects = new List<CharacterEffect>
                        {
                            Effect(
                                jensen,
                                Change(nameof(CurrentStats.TjenesteMotivation), 5),
                                Change(nameof(CurrentStats.Stress), 10)),

                            Effect(
                                hansen,
                                Change(nameof(CurrentStats.TjenesteMotivation), 10),
                                Change(nameof(CurrentStats.Stress), 20)),

                            Effect(
                                larsen,
                                Change(nameof(CurrentStats.Stress), 30),
                                Change(nameof(CurrentStats.Tillid), -10),
                                Change(nameof(CurrentStats.TjenesteMotivation), -15))
                        }
                    }
                }
            },

            new()
            {
                Text =
                    "Du får rapport om en mulig farezone. Hvordan håndterer du situationen?",

                AnswerOptions = new List<AnswerOption>
                {
                    new()
                    {
                        Text = "Giv klare instrukser og hold formation",

                        CharacterEffects = new List<CharacterEffect>
                        {
                            Effect(
                                jensen,
                                Change(nameof(CurrentStats.Tillid), 10),
                                Change(nameof(CurrentStats.Stress), -5)),

                            Effect(
                                hansen,
                                Change(nameof(CurrentStats.Tillid), 5),
                                Change(nameof(CurrentStats.Sociallyst), -5)),

                            Effect(
                                larsen,
                                Change(nameof(CurrentStats.Tillid), 15),
                                Change(nameof(CurrentStats.Stress), -15))
                        }
                    },

                    new()
                    {
                        Text = "Del gruppen op og send spejdere",

                        CharacterEffects = new List<CharacterEffect>
                        {
                            Effect(
                                jensen,
                                Change(nameof(CurrentStats.Tillid), 5),
                                Change(nameof(CurrentStats.Stress), 5)),

                            Effect(
                                hansen,
                                Change(nameof(CurrentStats.TjenesteMotivation), 15),
                                Change(nameof(CurrentStats.Stress), 10)),

                            Effect(
                                larsen,
                                Change(nameof(CurrentStats.Stress), 20),
                                Change(nameof(CurrentStats.Tillid), -10),
                                Change(nameof(CurrentStats.Sociallyst), -10))
                        }
                    }
                }
            }
        };
    }

    private static CharacterEffect Effect(
        Character character,
        params StatChange[] changes)
    {
        return new CharacterEffect
        {
            CharacterId = character.Id,
            Changes = changes.ToList()
        };
    }

    private static StatChange Change(
        string statName,
        int amount)
    {
        return new StatChange
        {
            StatName = statName,
            Amount = Math.Clamp(amount, -100, 100)
        };
    }
}