using Core;
using Backend.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var charRepo = scope.ServiceProvider.GetRequiredService<ICharacterRepository>();
        var qRepo = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();

        var existingChars = await charRepo.GetAllAsync();
        if (existingChars == null || existingChars.Count == 0)
        {
            var seeds = new List<Character>
            {
                new Character
                {
                    Name = "Sergent Jensen",
                    Description = "Erfaren og rolig leder. Forudsigelig og solid under pres.",
                    Traits = new CharacterTraits { Strukturbehov = 70, SocialtBehov = 50, Risikovillighed = 30, PresTolerance = 80, Fysisk = 60, Erfaringsniveau = 80 },
                    CurrentStats = new CurrentStats()
                },
                new Character
                {
                    Name = "Korporal Hansen",
                    Description = "Ung og energisk, søger socialt samspil. Klarer fysisk pres godt.",
                    Traits = new CharacterTraits { Strukturbehov = 40, SocialtBehov = 80, Risikovillighed = 50, PresTolerance = 60, Fysisk = 85, Erfaringsniveau = 40 },
                    CurrentStats = new CurrentStats()
                },
                new Character
                {
                    Name = "Specialist Larsen",
                    Description = "Teknisk dygtig, men lavt socialt behov. Kan blive stresset af uklare instrukser.",
                    Traits = new CharacterTraits { Strukturbehov = 80, SocialtBehov = 20, Risikovillighed = 30, PresTolerance = 70, Fysisk = 50, Erfaringsniveau = 70 },
                    CurrentStats = new CurrentStats()
                }
            };

            foreach (var c in seeds)
            {
                await charRepo.CreateAsync(c);
            }
        }

        var existingQuestions = await qRepo.GetAllAsync();
        if (existingQuestions == null || existingQuestions.Count == 0)
        {
            var q1 = new Question
            {
                Text = "Gruppen har været på øvelse i mange timer. Flere virker trætte, men missionen er ikke færdig. Hvad gør du?",
                AnswerOptions = new List<AnswerOption>
                {
                    new AnswerOption
                    {
                        Text = "Giv gruppen en kort pause",
                        Rules = new List<Rule>
                        {
                            new Rule { TargetGroup = StatGroup.Traits, ConditionStat = nameof(CharacterTraits.Fysisk), Operator = RuleOperator.LessThan, CompareValue = 50, EffectStat = nameof(CurrentStats.Stress), ChangeValue = -20 },
                            new Rule { TargetGroup = StatGroup.Traits, ConditionStat = nameof(CharacterTraits.PresTolerance), Operator = RuleOperator.LessThan, CompareValue = 50, EffectStat = nameof(CurrentStats.TjenesteMotivation), ChangeValue = +5 }
                        }
                    },
                    new AnswerOption
                    {
                        Text = "Pres gruppen videre",
                        Rules = new List<Rule>
                        {
                            new Rule { TargetGroup = StatGroup.Traits, ConditionStat = nameof(CharacterTraits.PresTolerance), Operator = RuleOperator.GreaterThan, CompareValue = 70, EffectStat = nameof(CurrentStats.TjenesteMotivation), ChangeValue = +10 },
                            new Rule { TargetGroup = StatGroup.Traits, ConditionStat = nameof(CharacterTraits.Fysisk), Operator = RuleOperator.LessThan, CompareValue = 50, EffectStat = nameof(CurrentStats.Stress), ChangeValue = +25 }
                        }
                    }
                }
            };

            var q2 = new Question
            {
                Text = "Du får rapport om en mulig farezone. Hvordan håndterer du det?",
                AnswerOptions = new List<AnswerOption>
                {
                    new AnswerOption
                    {
                        Text = "Giv klare instrukser og hold formation",
                        Rules = new List<Rule>
                        {
                            new Rule { TargetGroup = StatGroup.Traits, ConditionStat = nameof(CharacterTraits.Strukturbehov), Operator = RuleOperator.GreaterThan, CompareValue = 60, EffectStat = nameof(CurrentStats.Tillid), ChangeValue = +10 },
                            new Rule { TargetGroup = StatGroup.Traits, ConditionStat = nameof(CharacterTraits.Risikovillighed), Operator = RuleOperator.LessThan, CompareValue = 40, EffectStat = nameof(CurrentStats.Stress), ChangeValue = -10 }
                        }
                    },
                    new AnswerOption
                    {
                        Text = "Del gruppen op og send spejdere",
                        Rules = new List<Rule>
                        {
                            new Rule { TargetGroup = StatGroup.Traits, ConditionStat = nameof(CharacterTraits.Erfaringsniveau), Operator = RuleOperator.GreaterThan, CompareValue = 60, EffectStat = nameof(CurrentStats.Tillid), ChangeValue = +5 },
                            new Rule { TargetGroup = StatGroup.Traits, ConditionStat = nameof(CharacterTraits.SocialtBehov), Operator = RuleOperator.LessThan, CompareValue = 40, EffectStat = nameof(CurrentStats.Sociallyst), ChangeValue = -10 }
                        }
                    }
                }
            };

            await qRepo.CreateAsync(q1);
            await qRepo.CreateAsync(q2);
        }
    }
}

