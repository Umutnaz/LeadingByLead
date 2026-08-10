#!/usr/bin/env python3
"""Generate complete SeedData.cs from Excel data."""

import openpyxl
import os
import json
from typing import Dict, List, Tuple

wb = openpyxl.load_workbook('Følgeskab_parameter_model.xlsx')

def read_sheet_data(ws, start_row=2):
    """Read all rows from a worksheet starting at start_row."""
    headers = [cell.value for cell in ws[1]]
    data = []
    for row in ws.iter_rows(min_row=start_row, values_only=True):
        if all(v is None for v in row):
            break
        row_dict = {headers[i]: row[i] for i in range(len(headers))}
        data.append(row_dict)
    return data

# Read all data
start_values = read_sheet_data(wb[wb.sheetnames[1]])  # Startvrdier
gears = read_sheet_data(wb[wb.sheetnames[3]])  # Gear
action_cards = read_sheet_data(wb[wb.sheetnames[4]])  # Action cards
phase_events = read_sheet_data(wb[wb.sheetnames[2]])  # Fasehndelser

# Character ID mappings
CHAR_ID_TO_NAME = {
    'D': 'Daniel',
    'C': 'Charlie',
    'I': 'Inzo',
    'S': 'Søren',
    'IS': 'Ida-Sofie',
    'L': 'Lene'
}

CHAR_ID_TO_REF = {
    'D': 'daniel',
    'C': 'charlie',
    'I': 'inzo',
    'S': 'søren',
    'IS': 'ida',
    'L': 'lene'
}

ROLES = {
    'D': 'GV1',
    'C': 'LMG1',
    'I': 'GV3',
    'S': 'LMG2',
    'IS': 'GV2',
    'L': 'GV4'
}

def escape_csharp_string(s: str) -> str:
    """Escape a string for use in C# string literal."""
    if not s:
        return ""
    # Escape backslashes first, then quotes
    s = s.replace('\\', '\\\\')
    s = s.replace('"', '\\"')
    # Keep newlines as literal \n in the string
    s = s.replace('\n', '\\n')
    return s

def gen_change(stat_name: str, amount: int) -> str:
    """Generate a Change() call."""
    if amount == 0:
        return None
    return f'Change(nameof(CurrentStats.{stat_name}), {amount})'

def gen_effect(person_id: str, reaction: str, delta_tjeneste: int, delta_social: int, 
               delta_tillid: int, delta_stress: int) -> str:
    """Generate an Effect() call for a person."""
    char_ref = CHAR_ID_TO_REF.get(person_id)
    if not char_ref:
        return None
    
    changes = []
    if delta_tjeneste != 0:
        changes.append(gen_change('TjenesteMotivation', delta_tjeneste))
    if delta_social != 0:
        changes.append(gen_change('Sociallyst', delta_social))
    if delta_tillid != 0:
        changes.append(gen_change('Tillid', delta_tillid))
    if delta_stress != 0:
        changes.append(gen_change('Stress', delta_stress))
    
    changes = [c for c in changes if c]
    if not changes:
        return None
    
    reaction_escaped = escape_csharp_string(reaction)
    return f'Effect({char_ref}, "{reaction_escaped}", {", ".join(changes)})'

def gen_option(option_name: str, description: str, person_effects: List[str]) -> str:
    """Generate an Option() call."""
    description_escaped = escape_csharp_string(description)
    option_text_escaped = escape_csharp_string(option_name)
    
    effects_str = ',\n                    '.join(filter(None, person_effects))
    
    return f'''                Option(
                    "{option_text_escaped}\\n\\n{description_escaped}",

                    {effects_str}
                )'''

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

# Group phase events by phase
events_by_phase = {}
for event in phase_events:
    phase = event['Fase']
    if phase not in events_by_phase:
        events_by_phase[phase] = []
    events_by_phase[phase].append(event)

# Generate C# code
code = []
code.append('using Backend.Repositories;')
code.append('using Core;')
code.append('using Microsoft.Extensions.DependencyInjection;')
code.append('')
code.append('namespace Backend.Services;')
code.append('')
code.append('public static class SeedData')
code.append('{')
code.append('    public static async Task SeedAsync(IServiceProvider services)')
code.append('    {')
code.append('        using var scope = services.CreateScope();')
code.append('')
code.append('        var characterRepository =')
code.append('            scope.ServiceProvider.GetRequiredService<ICharacterRepository>();')
code.append('')
code.append('        var questionRepository =')
code.append('            scope.ServiceProvider.GetRequiredService<IQuestionRepository>();')
code.append('')
code.append('        var characters = await characterRepository.GetAllAsync();')
code.append('')
code.append('        if (characters.Count == 0)')
code.append('        {')
code.append('            characters = CreateCharacters();')
code.append('')
code.append('            foreach (var character in characters)')
code.append('            {')
code.append('                character.ResetCurrentStats();')
code.append('                await characterRepository.CreateAsync(character);')
code.append('            }')
code.append('        }')
code.append('')
code.append('        var questions = await questionRepository.GetAllAsync();')
code.append('')
code.append('        if (questions.Count == 0)')
code.append('        {')
code.append('            foreach (var question in CreateQuestions(characters))')
code.append('                await questionRepository.CreateAsync(question);')
code.append('        }')
code.append('    }')
code.append('')
code.append('    private static List<Character> CreateCharacters()')
code.append('    {')
code.append('        return new List<Character>')
code.append('        {')

# Add characters with correct starting values
for person in start_values:
    person_id = person['ID']
    name = CHAR_ID_TO_NAME.get(person_id, 'Unknown')
    role = ROLES.get(person_id, '')
    
    code.append('            new()')
    code.append('            {')
    code.append(f'                Name = "{name} - {role}",')
    code.append('                Description = "",')
    code.append('                BaseStats = new BaseStats')
    code.append('                {')
    code.append(f'                    TjenesteMotivation = {person["Tjenestemotivation"]},')
    code.append(f'                    Sociallyst = {person["Sociale lyst"]},')
    code.append(f'                    Tillid = {person["Tillid"]},')
    code.append(f'                    Stress = {person["Stress"]}')
    code.append('                }')
    code.append('            },')

code.append('        };')
code.append('    }')

# Now print the code so far
with open('seeddata_gen_part1.cs', 'w', encoding='utf-8') as f:
    f.write('\n'.join(code))

print("Part 1 (Characters) generated: seeddata_gen_part1.cs")
print(f"Total lines so far: {len(code)}")
print()

# Print statistics
print("=== DATA SUMMARY ===")
print(f"Starting values: {len(start_values)}")
print(f"Gears by (phase, number):")
for key in sorted(gears_by_phase_num.keys()):
    print(f"  {key}: {len(gears_by_phase_num[key])} impacts")

print(f"Action cards by (phase, number):")
for key in sorted(cards_by_phase_num.keys()):
    print(f"  {key}: {len(cards_by_phase_num[key])} impacts")

print(f"Phase events: {len(phase_events)}")
print()

# Save JSON dump for reference
with open('seeddata_dump.json', 'w', encoding='utf-8') as f:
    json.dump({
        'start_values': start_values,
        'gears': gears,
        'action_cards': action_cards,
        'phase_events': phase_events
    }, f, ensure_ascii=False, indent=2)

print("Data dumped to seeddata_dump.json")

