using Core;
using System;
using System.Collections.Generic;

namespace Backend.Services;

public static class RuleEvaluator
{
    public static void ApplyAnswerOptionToCharacters(List<Character> characters, AnswerOption option)
    {
        if (option?.Rules == null) return;

        foreach (var ch in characters)
        {
            foreach (var rule in option.Rules)
            {
                int left = GetStatValue(ch, rule.TargetGroup, rule.ConditionStat);
                bool condition = rule.Operator switch
                {
                    RuleOperator.LessThan => left < rule.CompareValue,
                    RuleOperator.GreaterThan => left > rule.CompareValue,
                    RuleOperator.Equal => left == rule.CompareValue,
                    _ => false
                };

                if (condition)
                {
                    int current = GetStatValue(ch, StatGroup.CurrentStats, rule.EffectStat);
                    int updated = current + rule.ChangeValue;
                    updated = Math.Clamp(updated, 0, 100);
                    SetCurrentStat(ch, rule.EffectStat, updated);
                }
            }
        }
    }

    static int GetStatValue(Character ch, StatGroup group, string statName)
    {
        if (group == StatGroup.Traits)
        {
            return statName switch
            {
                nameof(CharacterTraits.Strukturbehov) => ch.Traits.Strukturbehov,
                nameof(CharacterTraits.SocialtBehov) => ch.Traits.SocialtBehov,
                nameof(CharacterTraits.Risikovillighed) => ch.Traits.Risikovillighed,
                nameof(CharacterTraits.PresTolerance) => ch.Traits.PresTolerance,
                nameof(CharacterTraits.Fysisk) => ch.Traits.Fysisk,
                nameof(CharacterTraits.Erfaringsniveau) => ch.Traits.Erfaringsniveau,
                _ => 0
            };
        }
        else
        {
            return statName switch
            {
                nameof(CurrentStats.TjenesteMotivation) => ch.CurrentStats.TjenesteMotivation,
                nameof(CurrentStats.Stress) => ch.CurrentStats.Stress,
                nameof(CurrentStats.Sociallyst) => ch.CurrentStats.Sociallyst,
                nameof(CurrentStats.Tillid) => ch.CurrentStats.Tillid,
                _ => 0
            };
        }
    }

    static void SetCurrentStat(Character ch, string statName, int value)
    {
        switch (statName)
        {
            case nameof(CurrentStats.TjenesteMotivation): ch.CurrentStats.TjenesteMotivation = value; break;
            case nameof(CurrentStats.Stress): ch.CurrentStats.Stress = value; break;
            case nameof(CurrentStats.Sociallyst): ch.CurrentStats.Sociallyst = value; break;
            case nameof(CurrentStats.Tillid): ch.CurrentStats.Tillid = value; break;
            default: break;
        }
    }
}

