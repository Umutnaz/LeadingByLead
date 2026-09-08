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

        if (questions.Count < 6)
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
        var daniel = Find(characters, "Daniel");
        var charlie = Find(characters, "Charlie");
        var inzo = Find(characters, "Inzo");
        var soren = Find(characters, "Søren");
        var ida = Find(characters, "Ida-Sofie");
        var lene = Find(characters, "Lene");

        return new List<Question>
        {
            new()
            {
                Title = "Fase 1: Opstart",
                Description = "Ledelsen har besluttet, at din gruppe skal bestå af nye soldater og overflytninger fra andre grupper...",
                RequiredSelections = 1,
                AnswerOptions = new List<AnswerOption>
                {
                    Option(
                        "Minimalt socialt fokus\n\nIngen opstartsmøde; kendte processer implementeres hurtigt og fokus lægges på faglighed.",
                        Effect(daniel, "Positiv: effektivitet og konkrete processer", "Opgavefokus og handlekraft matcher Daniels præstationsbehov; lavt socialt fokus koster kun lidt.", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -2)),
                        Effect(charlie, "Positiv: effektivitet og konkrete processer", "Struktur og kendte standarder øger Charlies oplevede kontrol og reducerer usikkerhed.", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                        Effect(inzo, "Stærkt negativ: savner relationer", "Manglende tilhørsforhold rammer Inzos primære motivationskilde og svækker engagementet.", Change(nameof(CurrentStats.TjenesteMotivation), -9), Change(nameof(CurrentStats.Sociallyst), -10), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 10)),
                        Effect(soren, "Stærkt negativ: føler sig alene", "Lav relationel støtte kombineret med ny gruppe øger usikkerhed og oplevet belastning.", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -8), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 12)),
                        Effect(ida, "Stærkt negativ: savner relationer", "Som helt ny mister Ida-Sofie både social forankring og mulighed for at skabe ejerskab.", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -9), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 9)),
                        Effect(lene, "Svagt positiv: effektivitet", "Tydelighed hjælper, men den manglende relationsopbygning begrænser gevinsten.", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Tillid), 2), Change(nameof(CurrentStats.Stress), -1)))
                }
            },
            new()
            {
                Title = "Næste skridt i fase 1",
                Description = "Vælg tre action cards...",
                RequiredSelections = 3,
                AnswerOptions = new List<AnswerOption>
                {
                    Option(
                        "Fremtiden er lys\n\nMøde om gruppens fremtid med ensidigt fokus på styrker og positive muligheder.",
                        Effect(daniel, "Mildt negativ: savner realisme", "Ensided positivitet uden konkret plan reducerer troværdighed.", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 4)),
                        Effect(charlie, "Mildt negativ: savner konkret plan", "Manglende data, risici og struktur skader tillid til ledelsen.", Change(nameof(CurrentStats.TjenesteMotivation), -6), Change(nameof(CurrentStats.Tillid), -6), Change(nameof(CurrentStats.Stress), 5)),
                        Effect(inzo, "Stærkt positiv: begejstret for fremtidsbilledet", "Energi og positiv social framing skaber håb og engagement.", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                        Effect(soren, "Stærkt negativ: kan ikke se sin plads", "Uklar rolle og ren positiv framing øger usikkerhed og oplevet isolation.", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -4), Change(nameof(CurrentStats.Tillid), -8), Change(nameof(CurrentStats.Stress), 9)),
                        Effect(ida, "Stærkt positiv: begejstret for fremtidsbilledet", "Visionen understøtter kreativitet, mulighedstænkning og tilhørighed.", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                        Effect(lene, "Mildt negativ: savner realistisk billede", "Optimisme uden håndtering af bekymringer opleves som lav omsorg og troværdighed.", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 5)))
                }
            },
            new()
            {
                Title = "Fase 2: Drift",
                Description = "Gruppen er etableret. Nu skal de trænes til at håndtere komplekse scenarier...",
                RequiredSelections = 1,
                AnswerOptions = new List<AnswerOption>
                {
                    Option(
                        "Fælles refleksion\n\nTemperaturmåling og åben drøftelse af udfordringer.",
                        Effect(daniel, "Positiv: skaber klarhed", "Åben refleksion skaber mindre usikkerhed og mere handlekraft.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -2)),
                        Effect(charlie, "Positiv: struktur og læring", "Tydelig feedback og tydelig styringslogik styrker kvaliteten.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -2)),
                        Effect(inzo, "Positiv: føler sig hørt", "Fælles drøftelse styrker tilhørsforhold og engagement.", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 5), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                        Effect(soren, "Positiv: tryghed i samtale", "Det styrker psykologisk sikkerhed og reducerer overbelastning.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                        Effect(ida, "Positiv: kreativ og inkluderende", "Refleksion styrker både identitet og ejerskab.", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 5), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                        Effect(lene, "Positiv: relation og sikkerhed", "Lene får ro og mulighed for at løfte bekymringer.", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -2)))
                }
            },
            new()
            {
                Title = "Næste skridt i fase 2",
                Description = "Vælg tre action cards...",
                RequiredSelections = 3,
                AnswerOptions = new List<AnswerOption>
                {
                    Option(
                        "Vi gør det sammen\n\nGruppen drøfter, hvad der virker, hvad der ikke virker, og hvordan den kommer i mål inden for givne rammer.",
                        Effect(daniel, "Positiv: får medansvar for retningen", "Begrænset voice tilfredsstiller autonomi uden at fjerne målfokus.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -2)),
                        Effect(charlie, "Mildt negativ: retningen er stadig ukonkret", "Dialog uden tilstrækkelig beslutning og standarder opleves som ineffektiv.", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 4)),
                        Effect(inzo, "Positiv: dialog og fælles retning", "Involvering styrker både relation og ejerskab.", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                        Effect(soren, "Positiv: får tid og mulighed for at bidrage", "Voice og tydelige rammer øger psykologisk sikkerhed.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                        Effect(ida, "Positiv: forslag er velkomne", "Autonomi og kreativt bidrag styrker motivation og tillid.", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                        Effect(lene, "Positiv: fælles dialog inden for rammer", "Inklusion og tydelighed giver både relationel og procedural tillid.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)))
                }
            },
            new()
            {
                Title = "Fase 3: Overdragelse",
                Description = "Det er tid til at finde en ny fører. Hvilken strategi vælger du?",
                RequiredSelections = 1,
                AnswerOptions = new List<AnswerOption>
                {
                    Option(
                        "Klar lederrolle\n\nFøreren sætter retningen og instruerer soldaterne tydeligt.",
                        Effect(daniel, "Positiv: tydelig retning og ansvar", "Klar kommando reducerer uklarhed og matcher opgaveidentitet.", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                        Effect(charlie, "Positiv: tydelige standarder", "Rolle- og procesklarhed øger tillid og reducerer usikkerhed.", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                        Effect(inzo, "Stærkt negativ: føler sig overkørt", "Lav voice og relationel respekt reducerer tillid og social deltagelse.", Change(nameof(CurrentStats.TjenesteMotivation), -7), Change(nameof(CurrentStats.Sociallyst), -6), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 9)),
                        Effect(soren, "Stærkt negativ: føler sig trådt på", "Direktiv stil uden tryghed øger usikkerhed og hæmmer deltagelse.", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -5), Change(nameof(CurrentStats.Tillid), -11), Change(nameof(CurrentStats.Stress), 12)),
                        Effect(ida, "Stærkt negativ: føler sig overkørt", "Kontrollerende stil frustrerer autonomi og procedural retfærdighed.", Change(nameof(CurrentStats.TjenesteMotivation), -7), Change(nameof(CurrentStats.Sociallyst), -5), Change(nameof(CurrentStats.Tillid), -10), Change(nameof(CurrentStats.Stress), 10)),
                        Effect(lene, "Positiv: tydelighed", "Klar retning giver sikkerhed, selv om relationel involvering er lav.", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)))
                }
            },
            new()
            {
                Title = "Næste skridt i fase 3",
                Description = "Vælg tre action cards...",
                RequiredSelections = 3,
                AnswerOptions = new List<AnswerOption>
                {
                    Option(
                        "Lad os tale om det\n\nIndividuelle samtaler om situationen, behov og fælles præstation.",
                        Effect(daniel, "Positiv: egne behov anerkendes", "Kort, målrettet individuel dialog giver voice uden gruppemøde.", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -2)),
                        Effect(charlie, "Mildt negativ: vil videre", "Mere samtale uden nye fakta opleves som lav effektivitet.", Change(nameof(CurrentStats.TjenesteMotivation), -3), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 3)),
                        Effect(inzo, "Positiv: føler sig hørt", "Individuel opmærksomhed styrker relationel tillid og tilhørighed.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                        Effect(soren, "Positiv: føler sig hørt", "Samtalen reducerer usikkerhed og giver sikker kanal til bekymringer.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 7), Change(nameof(CurrentStats.Stress), -6)),
                        Effect(ida, "Positiv: behov og idéer anerkendes", "Autonomistøtte og relation øger tillid.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                        Effect(lene, "Positiv: relation og omsorg", "Individuel dialog passer til Lenes relationelle orientering.", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)))
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
