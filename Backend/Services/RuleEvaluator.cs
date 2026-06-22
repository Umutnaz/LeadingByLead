using Core;

namespace Backend.Services;

public static class RuleEvaluator
{
    public static void ApplyAnswerOptionToCharacters(
        List<Character> characters,
        AnswerOption option)
    {
        foreach (var effect in option.CharacterEffects)
        {
            var character = characters.FirstOrDefault(
                character => character.Id == effect.CharacterId);

            // Characters, der ikke er valgt til sessionen, ignoreres.
            if (character == null)
                continue;

            foreach (var change in effect.Changes)
            {
                ApplyChange(character, change);
            }
        }
    }

    private static void ApplyChange(
        Character character,
        StatChange change)
    {
        var amount = Math.Clamp(change.Amount, -100, 100);

        switch (change.StatName)
        {
            case nameof(CurrentStats.TjenesteMotivation):
                character.CurrentStats.TjenesteMotivation = Math.Clamp(
                    character.CurrentStats.TjenesteMotivation + amount,
                    0,
                    100);
                break;

            case nameof(CurrentStats.Stress):
                character.CurrentStats.Stress = Math.Clamp(
                    character.CurrentStats.Stress + amount,
                    0,
                    100);
                break;

            case nameof(CurrentStats.Sociallyst):
                character.CurrentStats.Sociallyst = Math.Clamp(
                    character.CurrentStats.Sociallyst + amount,
                    0,
                    100);
                break;

            case nameof(CurrentStats.Tillid):
                character.CurrentStats.Tillid = Math.Clamp(
                    character.CurrentStats.Tillid + amount,
                    0,
                    100);
                break;
        }
    }
}