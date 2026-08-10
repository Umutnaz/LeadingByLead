using Core;

namespace Backend.Services;

/// <summary>
/// Applies game rules including stat changes, multipliers, rounding, and engagement effect calculation.
/// </summary>
public static class RuleEvaluator
{
    /// <summary>
    /// Applies an answer option to all characters, with optional multiplier (e.g., for action card priority).
    /// Returns the engagement effect calculated from the applied changes.
    /// </summary>
    public static double ApplyAnswerOptionToCharacters(
        List<Character> characters,
        AnswerOption option,
        double multiplier = 1.0)
    {
        double engagementEffect = 0;

        foreach (var effect in option.CharacterEffects)
        {
            var character = characters.FirstOrDefault(
                character => character.Id == effect.CharacterId);

            // Characters, der ikke er valgt til sessionen, ignoreres.
            if (character == null)
                continue;

            // Store previous stats for later calculations
            var previousStats = new CurrentStats
            {
                TjenesteMotivation = character.CurrentStats.TjenesteMotivation,
                Stress = character.CurrentStats.Stress,
                Sociallyst = character.CurrentStats.Sociallyst,
                Tillid = character.CurrentStats.Tillid
            };

            // Apply all changes with multiplier
            foreach (var change in effect.Changes)
            {
                ApplyChange(character, change, multiplier);
            }

            // Calculate engagement effect for this character
            engagementEffect += CalculateEngagementEffect(previousStats, character.CurrentStats);
        }

        return engagementEffect;
    }

    /// <summary>
    /// Calculates the engagement effect based on the formula:
    /// 0.35 × Δ TjenesteMotivation + 0.20 × Δ SocialLyst + 0.30 × Δ Tillid − 0.15 × Δ Stress
    /// </summary>
    private static double CalculateEngagementEffect(CurrentStats beforeStats, CurrentStats afterStats)
    {
        var deltaTjenestemotivation = afterStats.TjenesteMotivation - beforeStats.TjenesteMotivation;
        var deltaSociallyst = afterStats.Sociallyst - beforeStats.Sociallyst;
        var deltaTillid = afterStats.Tillid - beforeStats.Tillid;
        var deltaStress = afterStats.Stress - beforeStats.Stress;

        return 0.35 * deltaTjenestemotivation +
               0.20 * deltaSociallyst +
               0.30 * deltaTillid -
               0.15 * deltaStress;
    }

    private static void ApplyChange(
        Character character,
        StatChange change,
        double multiplier = 1.0)
    {
        // Clamp the change amount to reasonable range
        var baseAmount = Math.Clamp(change.Amount, -100, 100);

        // Apply multiplier and round using AwayFromZero
        var roundedAmount = (int)Math.Round(baseAmount * multiplier, MidpointRounding.AwayFromZero);

        switch (change.StatName)
        {
            case nameof(CurrentStats.TjenesteMotivation):
                character.CurrentStats.TjenesteMotivation = Math.Clamp(
                    character.CurrentStats.TjenesteMotivation + roundedAmount,
                    0,
                    100);
                break;

            case nameof(CurrentStats.Stress):
                character.CurrentStats.Stress = Math.Clamp(
                    character.CurrentStats.Stress + roundedAmount,
                    0,
                    100);
                break;

            case nameof(CurrentStats.Sociallyst):
                character.CurrentStats.Sociallyst = Math.Clamp(
                    character.CurrentStats.Sociallyst + roundedAmount,
                    0,
                    100);
                break;

            case nameof(CurrentStats.Tillid):
                character.CurrentStats.Tillid = Math.Clamp(
                    character.CurrentStats.Tillid + roundedAmount,
                    0,
                    100);
                break;
        }
    }
}