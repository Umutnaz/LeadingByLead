#!/usr/bin/env python3
"""Generate complete SeedData.cs file with all impacts from Excel."""

import json

# Load the JSON dump created by previous script
with open('seeddata_dump.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

start_values = data['start_values']
gears = data['gears']
action_cards = data['action_cards']
phase_events = data['phase_events']

CHAR_ID_TO_NAME = {
    'D': 'Daniel', 'C': 'Charlie', 'I': 'Inzo',
    'S': 'Søren', 'IS': 'Ida-Sofie', 'L': 'Lene'
}

CHAR_ID_TO_REF = {
    'D': 'daniel', 'C': 'charlie', 'I': 'inzo',
    'S': 'søren', 'IS': 'ida', 'L': 'lene'
}

ROLES = {
    'D': 'GV1', 'C': 'LMG1', 'I': 'GV3',
    'S': 'LMG2', 'IS': 'GV2', 'L': 'GV4'
}

def esc(s):
    """Escape string for C# literal."""
    if not s:
        return ""
    s = str(s).replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n')
    return s

def gen_changes(tjenestemotivation, sociallyst, tillid, stress):
    """Generate list of Change() calls."""
    changes = []
    if tjenestemotivation != 0:
        changes.append(f"Change(nameof(CurrentStats.TjenesteMotivation), {tjenestemotivation})")
    if sociallyst != 0:
        changes.append(f"Change(nameof(CurrentStats.Sociallyst), {sociallyst})")
    if tillid != 0:
        changes.append(f"Change(nameof(CurrentStats.Tillid), {tillid})")
    if stress != 0:
        changes.append(f"Change(nameof(CurrentStats.Stress), {stress})")
    return changes

def gen_effect_line(person_id, reaction, explanation, tjenestemotivation, sociallyst, tillid, stress, indent="                    "):
    """Generate single Effect() line with explanation."""
    char_ref = CHAR_ID_TO_REF.get(person_id)
    if not char_ref:
        return None
    
    changes = gen_changes(tjenestemotivation, sociallyst, tillid, stress)
    if not changes:
        return None
    
    reaction_escaped = esc(reaction)
    explanation_escaped = esc(explanation)
    changes_str = ", ".join(changes)
    return f'{indent}Effect({char_ref}, "{reaction_escaped}", "{explanation_escaped}", {changes_str})'

# Build the file content
lines = [
    'using Backend.Repositories;',
    'using Core;',
    'using Microsoft.Extensions.DependencyInjection;',
    '',
    'namespace Backend.Services;',
    '',
    'public static class SeedData',
    '{',
    '    public static async Task SeedAsync(IServiceProvider services)',
    '    {',
    '        using var scope = services.CreateScope();',
    '',
    '        var characterRepository =',
    '            scope.ServiceProvider.GetRequiredService<ICharacterRepository>();',
    '',
    '        var questionRepository =',
    '            scope.ServiceProvider.GetRequiredService<IQuestionRepository>();',
    '',
    '        var characters = await characterRepository.GetAllAsync();',
    '',
    '        if (characters.Count == 0)',
    '        {',
    '            characters = CreateCharacters();',
    '',
    '            foreach (var character in characters)',
    '            {',
    '                character.ResetCurrentStats();',
    '                await characterRepository.CreateAsync(character);',
    '            }',
    '        }',
    '',
    '        var questions = await questionRepository.GetAllAsync();',
    '',
    '        if (questions.Count == 0)',
    '        {',
    '            foreach (var question in CreateQuestions(characters))',
    '                await questionRepository.CreateAsync(question);',
    '        }',
    '    }',
    '',
    '    private static List<Character> CreateCharacters()',
    '    {',
    '        return new List<Character>',
    '        {',
]

# Add characters
for person in start_values:
    pid = person['ID']
    name = CHAR_ID_TO_NAME.get(pid)
    role = ROLES.get(pid)
    
    lines.extend([
        '            new()',
        '            {',
        f'                Name = "{name} - {role}",',
        '                Description = "",',
        '                BaseStats = new BaseStats',
        '                {',
        f'                    TjenesteMotivation = {int(person["Tjenestemotivation"])},',
        f'                    Sociallyst = {int(person["Sociale lyst"])},',
        f'                    Tillid = {int(person["Tillid"])},',
        f'                    Stress = {int(person["Stress"])}',
        '                }',
        '            },',
    ])

lines.extend([
    '        };',
    '    }',
    '',
])

# Now create CreateQuestions method - this will be massive
# Organize data by phase

# Group gears by phase and number
gears_by_phase_num = {}
for gear in gears:
    key = (gear['Fase'], int(gear['Gear']))
    if key not in gears_by_phase_num:
        gears_by_phase_num[key] = []
    gears_by_phase_num[key].append(gear)

# Group action cards by phase and number
cards_by_phase_num = {}
for card in action_cards:
    key = (card['Fase'], int(card['Kort']))
    if key not in cards_by_phase_num:
        cards_by_phase_num[key] = []
    cards_by_phase_num[key].append(card)

# Start CreateQuestions
lines.extend([
    '    private static List<Question> CreateQuestions(List<Character> characters)',
    '    {',
    '        var søren = Find(characters, "Søren");',
    '        var ida = Find(characters, "Ida-Sofie");',
    '        var lene = Find(characters, "Lene");',
    '        var daniel = Find(characters, "Daniel");',
    '        var charlie = Find(characters, "Charlie");',
    '        var inzo = Find(characters, "Inzo");',
    '',
    '        return new List<Question>',
    '        {',
])

# For each phase, add a gear question and then action cards question
phases = ['Opstart', 'Drift', 'Overdragelse']
phase_nums = {'Opstart': 'PhaseOne', 'Drift': 'PhaseTwo', 'Overdragelse': 'PhaseThree'}

for phase in phases:
    phase_name = phase_nums[phase]
    
    # Add the gear question
    lines.append('            CreatePhase' + ('One' if phase == 'Opstart' else 'Two' if phase == 'Drift' else 'Three') + 'Question(')
    lines.append('                søren, ida, lene, daniel, charlie, inzo),')
    
    # Add the action cards question
    lines.append('            CreatePhase' + ('One' if phase == 'Opstart' else 'Two' if phase == 'Drift' else 'Three') + 'Actions(')
    lines.append('                søren, ida, lene, daniel, charlie, inzo),')

lines.append('        };')
lines.append('    }')
lines.append('')

# Now add the individual question creation methods
for phase in phases:
    phase_num = 'One' if phase == 'Opstart' else 'Two' if phase == 'Drift' else 'Three'
    
    lines.append(f'    private static Question CreatePhase{phase_num}Question(')
    lines.append('        Character søren, Character ida, Character lene,')
    lines.append('        Character daniel, Character charlie, Character inzo)')
    lines.append('    {')
    lines.append('        return new Question')
    lines.append('        {')
    lines.append('            RequiredSelections = 1,')
    lines.append('')
    
    # Add title and description based on phase
    if phase == 'Opstart':
        lines.append('            Title = "Fase 1: Opstart",')
        lines.append('            Description =')
        lines.append('                "Ledelsen har besluttet, at din gruppe skal bestå af nye ' +
                    'soldater og overflytninger fra andre grupper...",')
    elif phase == 'Drift':
        lines.append('            Title = "Fase 2: Drift",')
        lines.append('            Description =')
        lines.append('                "Gruppen er etableret. Nu skal de trænes til at håndtere ' +
                    'komplekse scenarioer...",')
    else:
        lines.append('            Title = "Fase 3: Overdragelse",')
        lines.append('            Description =')
        lines.append('                "Det er tid til at finde en ny fører. Hvilken strategi ' +
                    'vælger du?",')
    
    lines.append('            AnswerOptions = new List<AnswerOption>')
    lines.append('            {')
    
    # Add gear options for this phase
    for gear_num in range(1, 5):
        key = (phase, gear_num)
        if key in gears_by_phase_num:
            gear_group = gears_by_phase_num[key]
            # Get first entry for name and description
            first = gear_group[0]
            option_name = first.get('Gearnavn', f'Option {gear_num}')
            description = first.get('Beskrivelse', '')
            
            lines.append('                Option(')
            lines.append(f'                    "{esc(option_name)}\\n\\n{esc(description)}",')
            lines.append('')
            
            # Add effects for all 6 people in this gear
            for gear_item in gear_group:
                effect_line = gen_effect_line(
                    gear_item['ID'],
                    gear_item.get('Reaktion', ''),
                    gear_item.get('Psykologisk begrundelse', ''),
                    gear_item.get('Δ Tjenestemotivation', 0) or 0,
                    gear_item.get('Δ Sociale lyst', 0) or 0,
                    gear_item.get('Δ Tillid', 0) or 0,
                    gear_item.get('Δ Stress', 0) or 0
                )
                if effect_line:
                    lines.append(effect_line + ',')
            
            lines.append('                ),')
            lines.append('')
    
    lines.append('            }')
    lines.append('        };')
    lines.append('    }')
    lines.append('')
    
    # Add the action cards question
    lines.append(f'    private static Question CreatePhase{phase_num}Actions(')
    lines.append('        Character søren, Character ida, Character lene,')
    lines.append('        Character daniel, Character charlie, Character inzo)')
    lines.append('    {')
    lines.append('        return new Question')
    lines.append('        {')
    lines.append('            RequiredSelections = 3,')
    lines.append('')
    lines.append(f'            Title = "Næste skridt i fase {["1", "2", "3"][["Opstart", "Drift", "Overdragelse"].index(phase)]}",')
    lines.append('            Description = "Vælg tre action cards...",')
    lines.append('            AnswerOptions = new List<AnswerOption>')
    lines.append('            {')
    
    # Add action card options for this phase
    for card_num in range(1, 9):
        key = (phase, card_num)
        if key in cards_by_phase_num:
            card_group = cards_by_phase_num[key]
            first = card_group[0]
            card_name = first.get('Kortnavn', f'Card {card_num}')
            description = first.get('Beskrivelse', '')
            
            lines.append('                Option(')
            lines.append(f'                    "{esc(card_name)}\\n\\n{esc(description)}",')
            lines.append('')
            
            # Add effects for all 6 people
            for card_item in card_group:
                effect_line = gen_effect_line(
                    card_item['ID'],
                    card_item.get('Reaktion', ''),
                    card_item.get('Psykologisk begrundelse', ''),
                    card_item.get('Δ Tjenestemotivation', 0) or 0,
                    card_item.get('Δ Sociale lyst', 0) or 0,
                    card_item.get('Δ Tillid', 0) or 0,
                    card_item.get('Δ Stress', 0) or 0
                )
                if effect_line:
                    lines.append(effect_line + ',')
            
            lines.append('                ),')
            lines.append('')
    
    lines.append('            }')
    lines.append('        };')
    lines.append('    }')
    lines.append('')

# Add helper methods
lines.extend([
    '    private static Character Find(List<Character> characters, string name)',
    '    {',
    '        return characters.First(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));',
    '    }',
    '',
    '    private static AnswerOption Option(string text, params CharacterEffect[] effects)',
    '    {',
    '        return new AnswerOption',
    '        {',
    '            Text = text,',
    '            CharacterEffects = effects.Where(e => e.Changes.Count > 0).ToList()',
    '        };',
    '    }',
    '',
    '    private static CharacterEffect Effect(Character character, string reaction, params StatChange[] changes)',
    '    {',
    '        return new CharacterEffect',
    '        {',
    '            CharacterId = character.Id,',
    '            Reaction = reaction,',
    '            Changes = changes.Where(c => c.Amount != 0).ToList()',
    '        };',
    '    }',
    '',
    '    private static StatChange Change(string statName, int amount)',
    '    {',
    '        return new StatChange { StatName = statName, Amount = amount };',
    '    }',
    '}',
])

# Write the file
output_path = 'Backend/Services/SeedData.cs'
with open(output_path, 'w', encoding='utf-8') as f:
    f.write('\n'.join(lines))

print(f"Generated {output_path}")
print(f"Total lines: {len(lines)}")

# Print summary
print(f"\n=== FILE STRUCTURE ===")
print(f"- Characters: {len(start_values)}")
print(f"- Gear questions: 3 (one per phase with 4 options each)")
print(f"- Action card questions: 3 (one per phase with 8 options each)")
print(f"- Total question methods: 6")

