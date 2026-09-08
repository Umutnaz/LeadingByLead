using System.Text.Json;
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
            {
                await questionRepository.CreateAsync(question);
            }
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
            }
        };
    }

    private static List<Question> CreateQuestions(List<Character> characters)
    {
        var questions = new List<Question>();

        var root = TryLoadSeedJson();
        if (root == null)
        {
            return CreateFallbackQuestions(characters);
        }

        var rootValue = root.Value;

        var phaseMap = new Dictionary<string, string>
        {
            ["Opstart"] = "Fase 1: Opstart",
            ["Drift"] = "Fase 2: Drift",
            ["Overdragelse"] = "Fase 3: Overdragelse"
        };

        var phaseDescriptions = new Dictionary<string, string>
        {
            ["Opstart"] = "Ledelsen har besluttet, at din gruppe skal bestå af nye soldater og overflytninger fra andre grupper...",
            ["Drift"] = "Gruppen er etableret. Nu skal de trænes til at håndtere komplekse scenarier...",
            ["Overdragelse"] = "Det er tid til at finde en ny fører. Hvilken strategi vælger du?"
        };

        foreach (var phaseName in new[] { "Opstart", "Drift", "Overdragelse" })
        {
            var phaseQuestion = new Question
            {
                Title = phaseMap[phaseName],
                Description = phaseDescriptions[phaseName],
                RequiredSelections = 1,
                AnswerOptions = BuildAnswerOptions(characters, rootValue, phaseName, false)
            };

            var phaseActions = new Question
            {
                Title = phaseName switch
                {
                    "Opstart" => "Næste skridt i fase 1",
                    "Drift" => "Næste skridt i fase 2",
                    _ => "Næste skridt i fase 3"
                },
                Description = "Vælg tre action cards...",
                RequiredSelections = 3,
                AnswerOptions = BuildAnswerOptions(characters, rootValue, phaseName, true)
            };

            questions.Add(phaseQuestion);
            questions.Add(phaseActions);
        }

        return questions;
    }

    private static List<AnswerOption> BuildAnswerOptions(List<Character> characters, JsonElement root, string phaseName, bool isAction)
    {
        var records = root.TryGetProperty(isAction ? "action_cards" : "gears", out var array)
            ? array
            : default;

        if (records.ValueKind != JsonValueKind.Array)
        {
            return new List<AnswerOption>();
        }

        var groups = new Dictionary<int, List<JsonElement>>();

        foreach (var item in records.EnumerateArray())
        {
            if (!item.TryGetProperty("Fase", out var phase) || phase.GetString() != phaseName)
            {
                continue;
            }

            var key = isAction ? item.GetProperty("Kort").GetInt32() : item.GetProperty("Gear").GetInt32();
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<JsonElement>();
                groups[key] = list;
            }

            list.Add(item.Clone());
        }

        var options = new List<AnswerOption>();

        foreach (var ordered in groups.OrderBy(kvp => kvp.Key))
        {
            var entries = ordered.Value;
            var first = entries[0];

            var text = isAction
                ? $"{first.GetProperty("Kortnavn").GetString()}\n\n{first.GetProperty("Beskrivelse").GetString()}"
                : $"{first.GetProperty("Gearnavn").GetString()}\n\n{first.GetProperty("Beskrivelse").GetString()}";

            var effects = new List<CharacterEffect>();

            foreach (var entry in entries)
            {
                var personName = entry.GetProperty("Person").GetString();
                var character = characters.FirstOrDefault(c => c.Name.Contains(personName ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                if (character == null)
                {
                    continue;
                }

                var reaction = entry.TryGetProperty("Reaktion", out var reactionProp)
                    ? reactionProp.GetString() ?? string.Empty
                    : string.Empty;

                var explanation = entry.TryGetProperty("Psykologisk begrundelse", out var explanationProp)
                    ? explanationProp.GetString() ?? string.Empty
                    : string.Empty;

                var changes = new List<StatChange>();
                AddChange(changes, nameof(CurrentStats.TjenesteMotivation), entry, "Δ Tjenestemotivation");
                AddChange(changes, nameof(CurrentStats.Sociallyst), entry, "Δ Sociale lyst");
                AddChange(changes, nameof(CurrentStats.Tillid), entry, "Δ Tillid");
                AddChange(changes, nameof(CurrentStats.Stress), entry, "Δ Stress");

                effects.Add(new CharacterEffect
                {
                    CharacterId = character.Id,
                    Reaction = reaction,
                    Explanation = explanation,
                    Changes = changes
                });
            }

            options.Add(Option(text, effects.ToArray()));
        }

        return options;
    }

    private static void AddChange(List<StatChange> changes, string propertyName, JsonElement entry, string jsonKey)
    {
        if (!entry.TryGetProperty(jsonKey, out var value))
        {
            return;
        }

        var amount = value.TryGetInt32(out var parsed) ? parsed : 0;
        if (amount == 0)
        {
            return;
        }

        changes.Add(new StatChange
        {
            StatName = propertyName,
            Amount = amount
        });
    }

    private static AnswerOption Option(string text, params CharacterEffect[] effects)
    {
        return new AnswerOption
        {
            Text = text,
            CharacterEffects = effects
                .Where(e => e.Changes.Count > 0)
                .ToList()
        };
    }

    private static JsonElement? TryLoadSeedJson()
    {
        var candidatePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "seeddata_dump.json"),
            Path.Combine(AppContext.BaseDirectory, "seeddata_dump.json"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "seeddata_dump.json"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "seeddata_dump.json"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "seeddata_dump.json")
        };

        foreach (var candidate in candidatePaths)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
            {
                try
                {
                    using var stream = File.OpenRead(fullPath);
                    return JsonDocument.Parse(stream).RootElement.Clone();
                }
                catch
                {
                    return null;
                }
            }
        }

        return null;
    }

    private static List<Question> CreateFallbackQuestions(List<Character> characters)
    {
        return new List<Question>
        {
            new()
            {
                Title = "Fase 1: Opstart",
                Description = "Fallback seed",
                RequiredSelections = 1,
                AnswerOptions = new List<AnswerOption>
                {
                    Option(
                        "Fallback",
                        Effect(Find(characters, "Daniel"), "Positiv: fallback", "Fallback explanation", Change(nameof(CurrentStats.TjenesteMotivation), 1)))
                }
            }
        };
    }

    private static Character Find(List<Character> characters, string name)
    {
        return characters.First(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    private static CharacterEffect Effect(Character character, string reaction, string explanation, params StatChange[] changes)
    {
        return new CharacterEffect
        {
            CharacterId = character.Id,
            Reaction = reaction,
            Explanation = explanation,
            Changes = changes.Where(c => c.Amount != 0).ToList()
        };
    }

    private static StatChange Change(string statName, int amount)
    {
        return new StatChange { StatName = statName, Amount = amount };
    }
}
