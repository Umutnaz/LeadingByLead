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

    private static List<Question> CreateQuestions(List<Character> characters)
    {
        var søren = Find(characters, "Søren");
        var ida = Find(characters, "Ida-Sofie");
        var lene = Find(characters, "Lene");
        var daniel = Find(characters, "Daniel");
        var charlie = Find(characters, "Charlie");
        var inzo = Find(characters, "Inzo");

        return new List<Question>
        {
            CreatePhaseOneQuestion(
                søren, ida, lene, daniel, charlie, inzo),
            CreatePhaseOneActions(
                søren, ida, lene, daniel, charlie, inzo),
            CreatePhaseTwoQuestion(
                søren, ida, lene, daniel, charlie, inzo),
            CreatePhaseTwoActions(
                søren, ida, lene, daniel, charlie, inzo),
            CreatePhaseThreeQuestion(
                søren, ida, lene, daniel, charlie, inzo),
            CreatePhaseThreeActions(
                søren, ida, lene, daniel, charlie, inzo),
        };
    }

    private static Question CreatePhaseOneQuestion(
        Character søren, Character ida, Character lene,
        Character daniel, Character charlie, Character inzo)
    {
        return new Question
        {
            RequiredSelections = 1,

            Title = "Fase 1: Opstart",
            Description =
                "Ledelsen har besluttet, at din gruppe skal bestå af nye soldater og overflytninger fra andre grupper...",
            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "Minimalt socialt fokus\n\nIngen opstartsmøde; kendte processer implementeres hurtigt og fokus lægges på faglighed.",

                    Effect(daniel, "Positiv: effektivitet og konkrete processer", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -2)),
                    Effect(charlie, "Positiv: effektivitet og konkrete processer", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(inzo, "Stærkt negativ: savner relationer", Change(nameof(CurrentStats.TjenesteMotivation), -9), Change(nameof(CurrentStats.Sociallyst), -10), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 10)),
                    Effect(søren, "Stærkt negativ: føler sig alene", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -8), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 12)),
                    Effect(ida, "Stærkt negativ: savner relationer", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -9), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 9)),
                    Effect(lene, "Svagt positiv: effektivitet", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Tillid), 2), Change(nameof(CurrentStats.Stress), -1)),
                ),

                Option(
                    "Mindre socialt fokus\n\nKort præsentation og AKOS; processer drøftes kun begrænset, hvorefter gruppen går hurtigt i felten.",

                    Effect(daniel, "Positiv: mål og kort involvering", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(charlie, "Positiv: struktur og fremdrift", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -2)),
                    Effect(inzo, "Mildt negativ: for lidt relationsarbejde", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -5), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(søren, "Stærkt negativ: mødet opleves som proforma", Change(nameof(CurrentStats.TjenesteMotivation), -7), Change(nameof(CurrentStats.Sociallyst), -6), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 10)),
                    Effect(ida, "Mildt negativ: for lidt involvering", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -5), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(lene, "Positiv: mål og begrænset dialog", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -2)),
                ),

                Option(
                    "Mere socialt fokus\n\nGod tid til at lære hinanden at kende, løbende dialog og pauser for at få alle med.",

                    Effect(daniel, "Mildt negativ: mister tålmodighed", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(charlie, "Mildt negativ: opgaven mister fokus", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(inzo, "Positiv: socialt fokus og lytten", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 8), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(søren, "Positiv: støtte og ro", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 5), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -6)),
                    Effect(ida, "Positiv: socialt fokus og indflydelse", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 7), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(lene, "Mildt negativ: bekymring for opgaven", Change(nameof(CurrentStats.TjenesteMotivation), -3), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 4)),
                ),

                Option(
                    "Maksimalt socialt fokus\n\nTo hele sociale weekender, teori og hygge prioriteres frem for feltdage og faglig fremdrift.",

                    Effect(daniel, "Meget negativ: identitets- og formålstab", Change(nameof(CurrentStats.TjenesteMotivation), -18), Change(nameof(CurrentStats.Sociallyst), -6), Change(nameof(CurrentStats.Tillid), -12), Change(nameof(CurrentStats.Stress), 15)),
                    Effect(charlie, "Meget negativ: kvalitet og formål forsvinder", Change(nameof(CurrentStats.TjenesteMotivation), -17), Change(nameof(CurrentStats.Sociallyst), -5), Change(nameof(CurrentStats.Tillid), -14), Change(nameof(CurrentStats.Stress), 16)),
                    Effect(inzo, "Positiv: maksimalt socialt fokus", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 9), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Positiv: trygt læringsmiljø", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 7), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(ida, "Positiv: socialt og fleksibelt miljø", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 9), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Mildt negativ: kvalitet kan blive negligeret", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                ),

            }
        };
    }

    private static Question CreatePhaseOneActions(
        Character søren, Character ida, Character lene,
        Character daniel, Character charlie, Character inzo)
    {
        return new Question
        {
            RequiredSelections = 3,

            Title = "Næste skridt i fase 1",
            Description = "Vælg tre action cards...",
            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "Fremtiden er lys\n\nMøde om gruppens fremtid med ensidigt fokus på styrker og positive muligheder.",

                    Effect(daniel, "Mildt negativ: savner realisme", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(charlie, "Mildt negativ: savner konkret plan", Change(nameof(CurrentStats.TjenesteMotivation), -6), Change(nameof(CurrentStats.Tillid), -6), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(inzo, "Stærkt positiv: begejstret for fremtidsbilledet", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(søren, "Stærkt negativ: kan ikke se sin plads", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -4), Change(nameof(CurrentStats.Tillid), -8), Change(nameof(CurrentStats.Stress), 9)),
                    Effect(ida, "Stærkt positiv: begejstret for fremtidsbilledet", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(lene, "Mildt negativ: savner realistisk billede", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 5)),
                ),

                Option(
                    "Vi gør det sammen\n\nGruppen drøfter, hvad der virker, hvad der ikke virker, og hvordan den kommer i mål inden for givne rammer.",

                    Effect(daniel, "Positiv: får medansvar for retningen", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -2)),
                    Effect(charlie, "Mildt negativ: retningen er stadig ukonkret", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(inzo, "Positiv: dialog og fælles retning", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Positiv: får tid og mulighed for at bidrage", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(ida, "Positiv: forslag er velkomne", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: fælles dialog inden for rammer", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                ),

                Option(
                    "Kend din gruppe\n\nAlle præsenterer sig selv og opfordres til at interagere.",

                    Effect(daniel, "Mildt negativ: opleves som uproduktivt", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(charlie, "Mildt negativ: ukonkret og ustruktureret", Change(nameof(CurrentStats.TjenesteMotivation), -6), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 6)),
                    Effect(inzo, "Stærkt positiv: elsker præsentationen", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 8), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Meget negativ: føler sig udstillet", Change(nameof(CurrentStats.TjenesteMotivation), -10), Change(nameof(CurrentStats.Sociallyst), -8), Change(nameof(CurrentStats.Tillid), -12), Change(nameof(CurrentStats.Stress), 15)),
                    Effect(ida, "Stærkt positiv: elsker præsentationen", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 8), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Mildt negativ: ukonkret og ustruktureret", Change(nameof(CurrentStats.TjenesteMotivation), -3), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 4)),
                ),

                Option(
                    "Jeg har kommandoen\n\nFøreren sætter retningen og instruerer soldaterne tydeligt.",

                    Effect(daniel, "Positiv: tydelig retning og ansvar", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(charlie, "Positiv: tydelige standarder", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(inzo, "Stærkt negativ: føler sig overkørt", Change(nameof(CurrentStats.TjenesteMotivation), -7), Change(nameof(CurrentStats.Sociallyst), -6), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 9)),
                    Effect(søren, "Stærkt negativ: føler sig trådt på", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -5), Change(nameof(CurrentStats.Tillid), -11), Change(nameof(CurrentStats.Stress), 12)),
                    Effect(ida, "Stærkt negativ: føler sig overkørt", Change(nameof(CurrentStats.TjenesteMotivation), -7), Change(nameof(CurrentStats.Sociallyst), -5), Change(nameof(CurrentStats.Tillid), -10), Change(nameof(CurrentStats.Stress), 10)),
                    Effect(lene, "Positiv: tydelighed", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                ),

                Option(
                    "Lad os tale om det\n\nIndividuelle samtaler om situationen, behov og fælles præstation.",

                    Effect(daniel, "Positiv: egne behov anerkendes", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -2)),
                    Effect(charlie, "Mildt negativ: vil videre", Change(nameof(CurrentStats.TjenesteMotivation), -3), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 3)),
                    Effect(inzo, "Positiv: føler sig hørt", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Positiv: føler sig hørt", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 7), Change(nameof(CurrentStats.Stress), -6)),
                    Effect(ida, "Positiv: behov og idéer anerkendes", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(lene, "Positiv: relation og omsorg", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                ),

                Option(
                    "Lad os prøve\n\nGruppen får lov at handle og fejle uden mange kunstpauser.",

                    Effect(daniel, "Positiv: kommer i gang og udfordres", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), 1)),
                    Effect(charlie, "Mildt negativ: tempo uden kontrol", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 7)),
                    Effect(inzo, "Positiv: variation og handling", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), 1)),
                    Effect(søren, "Stærkt negativ: søger tryghed", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -4), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 12)),
                    Effect(ida, "Stærkt negativ: føler sig ikke tryg", Change(nameof(CurrentStats.TjenesteMotivation), -7), Change(nameof(CurrentStats.Sociallyst), -4), Change(nameof(CurrentStats.Tillid), -8), Change(nameof(CurrentStats.Stress), 10)),
                    Effect(lene, "Mildt negativ: tempoet bliver for højt", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 7)),
                ),

                Option(
                    "Træk på kompetencer\n\nStabile soldater får delegeret opgaver, så deres faglighed og engagement anerkendes.",

                    Effect(daniel, "Positiv: kompetence og ansvar anerkendes", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(charlie, "Positiv: faglighed anerkendes", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(inzo, "Positiv: bidrag bliver synligt", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(søren, "Positiv: stabil indsats anerkendes", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(ida, "Positiv: kompetencer bruges", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: engagement anerkendes", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                ),

                Option(
                    "Jeg har brug for dig\n\nSoldater, der har mistet knisten, får kritiske opgaver og ansvar.",

                    Effect(daniel, "Stærkt negativ: føler sig ikke involveret", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -8), Change(nameof(CurrentStats.Stress), 8)),
                    Effect(charlie, "Stærkt negativ: kompetence overses", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 9)),
                    Effect(inzo, "Positiv: bliver aktiveret", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Positiv: bliver aktiveret", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(ida, "Positiv: får ansvar", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: får en vigtig rolle", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                ),

            }
        };
    }

    private static Question CreatePhaseTwoQuestion(
        Character søren, Character ida, Character lene,
        Character daniel, Character charlie, Character inzo)
    {
        return new Question
        {
            RequiredSelections = 1,

            Title = "Fase 2: Drift",
            Description =
                "Gruppen er etableret. Nu skal de trænes til at håndtere komplekse scenarioer...",
            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "Minimal kompleksitet\n\nTempoet reduceres til grundlæggende niveau, og avancerede scenarier udskydes til alle er med.",

                    Effect(daniel, "Mildt negativ: tvivl om ambitionerne", Change(nameof(CurrentStats.TjenesteMotivation), -6), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(charlie, "Stærkt negativ: oplever tidsspild", Change(nameof(CurrentStats.TjenesteMotivation), -11), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -8), Change(nameof(CurrentStats.Stress), 9)),
                    Effect(inzo, "Svagt positiv: gruppen holdes samlet", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(søren, "Positiv: alle får mulighed for at følge med", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -7)),
                    Effect(ida, "Svagt positiv: mindre pres", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: omsorg for de svageste", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -6)),
                ),

                Option(
                    "Moderat kompleksitet\n\nMålrettet støtte, gradvist stigende niveau og kontrolleret introduktion af nye opgaver.",

                    Effect(daniel, "Positiv: accepterer niveauet", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -2)),
                    Effect(charlie, "Positiv: kontrolleret progression", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(inzo, "Mildt negativ: savner variation", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -2), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(søren, "Blandet positiv: udfordret men med", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Tillid), 2), Change(nameof(CurrentStats.Stress), 2)),
                    Effect(ida, "Mildt negativ: kreativiteten begrænses", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(lene, "Mildt negativ: tempoet opleves højt", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 6)),
                ),

                Option(
                    "Høj kompleksitet\n\nKomplekse realistiske opgaver bliver normen, selv om ikke alle kan følge med hele tiden.",

                    Effect(daniel, "Meget positiv: elsker tempoet", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), 1)),
                    Effect(charlie, "Meget positiv: elsker kompleksiteten", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), 2)),
                    Effect(inzo, "Mildt negativ: ser ikke meningen med detaljerne", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(søren, "Stærkt negativ: usikker på egne evner", Change(nameof(CurrentStats.TjenesteMotivation), -7), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 10)),
                    Effect(ida, "Blandet: udfordret og motiveret, men presset", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Tillid), 1), Change(nameof(CurrentStats.Stress), 8)),
                    Effect(lene, "Stærkt negativ: bekymret for de svageste", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -4), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 11)),
                ),

                Option(
                    "Maksimal kompleksitet\n\nUdelukkende opgavefokus, højt tempo og træning som om regionsøvelsen var næste måned.",

                    Effect(daniel, "Meget positiv: i sit es", Change(nameof(CurrentStats.TjenesteMotivation), 8), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), 2)),
                    Effect(charlie, "Meget positiv: i sit es", Change(nameof(CurrentStats.TjenesteMotivation), 8), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(inzo, "Stærkt negativ: fællesskabet forsvinder", Change(nameof(CurrentStats.TjenesteMotivation), -9), Change(nameof(CurrentStats.Sociallyst), -8), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 9)),
                    Effect(søren, "Stærkt negativ: føler sig overhalet", Change(nameof(CurrentStats.TjenesteMotivation), -11), Change(nameof(CurrentStats.Sociallyst), -4), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 15)),
                    Effect(ida, "Meget negativ: mister motivationen", Change(nameof(CurrentStats.TjenesteMotivation), -18), Change(nameof(CurrentStats.Sociallyst), -6), Change(nameof(CurrentStats.Tillid), -12), Change(nameof(CurrentStats.Stress), 18)),
                    Effect(lene, "Stærkt negativ: bekymret for fællesskab og kapacitet", Change(nameof(CurrentStats.TjenesteMotivation), -9), Change(nameof(CurrentStats.Sociallyst), -8), Change(nameof(CurrentStats.Tillid), -8), Change(nameof(CurrentStats.Stress), 12)),
                ),

            }
        };
    }

    private static Question CreatePhaseTwoActions(
        Character søren, Character ida, Character lene,
        Character daniel, Character charlie, Character inzo)
    {
        return new Question
        {
            RequiredSelections = 3,

            Title = "Næste skridt i fase 2",
            Description = "Vælg tre action cards...",
            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "Makkerordning\n\nNye og erfarne soldater kobles sammen, og de erfarne får udviklingsansvar.",

                    Effect(daniel, "Positiv: udviklingsansvar", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -2)),
                    Effect(charlie, "Mildt negativ: går ud over effektivitet", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(inzo, "Svagt positiv: mere kontakt", Change(nameof(CurrentStats.TjenesteMotivation), 1), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 1), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(søren, "Positiv: støtte og gradvis læring", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -6)),
                    Effect(ida, "Positiv: støtte fra erfaren soldat", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(lene, "Positiv: føler sig støttet", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -5)),
                ),

                Option(
                    "Brug dine styrker\n\nRoller og ansvar tilpasses den enkeltes styrker.",

                    Effect(daniel, "Stærkt positiv: styrker og ansvar matches", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(charlie, "Stærkt positiv: faglig specialisering", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(inzo, "Stærkt positiv: får en passende rolle", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Let negativ: rollen føles krævende", Change(nameof(CurrentStats.TjenesteMotivation), -1), Change(nameof(CurrentStats.Tillid), -1), Change(nameof(CurrentStats.Stress), 3)),
                    Effect(ida, "Stærkt positiv: kreativitet og autonomi", Change(nameof(CurrentStats.TjenesteMotivation), 8), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 7), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(lene, "Neutral/besværlig: usikker på specialisering", Change(nameof(CurrentStats.Stress), 2)),
                ),

                Option(
                    "Vi skruer op\n\nSværhedsgraden øges markant, og gruppen træner over niveau.",

                    Effect(daniel, "Positiv: elsker den høje udfordring", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), 2)),
                    Effect(charlie, "Positiv: elsker kompleksiteten", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), 3)),
                    Effect(inzo, "Svagt positiv/belastende", Change(nameof(CurrentStats.TjenesteMotivation), 1), Change(nameof(CurrentStats.Stress), 2)),
                    Effect(søren, "Stærkt negativ: føler sig presset", Change(nameof(CurrentStats.TjenesteMotivation), -9), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -8), Change(nameof(CurrentStats.Stress), 14)),
                    Effect(ida, "Stærkt negativ: føler sig presset", Change(nameof(CurrentStats.TjenesteMotivation), -7), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -6), Change(nameof(CurrentStats.Stress), 11)),
                    Effect(lene, "Stærkt negativ: føler sig presset", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 12)),
                ),

                Option(
                    "Hvad siger gruppen?\n\nTemperaturmåling og åben drøftelse af udfordringer.",

                    Effect(daniel, "Mildt negativ: for meget snak", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 3)),
                    Effect(charlie, "Neutral/svagt positiv: giver data", Change(nameof(CurrentStats.Tillid), 1), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(inzo, "Positiv: føler sig hørt", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Positiv: føler sig hørt", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 7), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(ida, "Positiv: får indflydelse", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: gruppens kapacitet anerkendes", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                ),

                Option(
                    "Fejl er læring\n\nFejl accepteres og evalueres åbent efter aktiviteter.",

                    Effect(daniel, "Svagt positiv: hurtig læringscyklus", Change(nameof(CurrentStats.TjenesteMotivation), 1), Change(nameof(CurrentStats.Tillid), 1), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(charlie, "Mildt negativ: frygter lave standarder", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(inzo, "Positiv: værdsætter kulturen", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Positiv: fejl bliver mindre truende", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 7), Change(nameof(CurrentStats.Stress), -7)),
                    Effect(ida, "Positiv: tryghed til at eksperimentere", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 7), Change(nameof(CurrentStats.Stress), -6)),
                    Effect(lene, "Positiv: mindre skyld og mere støtte", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -3)),
                ),

                Option(
                    "Talenter i spidsen\n\nDe dygtigste driver dele af undervisningen.",

                    Effect(daniel, "Stærkt positiv: får tillid og ansvar", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(charlie, "Stærkt positiv: fagligheden anerkendes", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(inzo, "Mildt negativ: føler sig sat udenfor", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -5), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 6)),
                    Effect(søren, "Mildt negativ: føler sig utilstrækkelig", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -6), Change(nameof(CurrentStats.Stress), 8)),
                    Effect(ida, "Neutral/svagt positiv", Change(nameof(CurrentStats.TjenesteMotivation), 1), Change(nameof(CurrentStats.Tillid), 1), Change(nameof(CurrentStats.Stress), 1)),
                    Effect(lene, "Neutral/svagt positiv", Change(nameof(CurrentStats.TjenesteMotivation), 1), Change(nameof(CurrentStats.Tillid), 1), Change(nameof(CurrentStats.Stress), 1)),
                ),

                Option(
                    "Vis mig resultater\n\nKonkrete mål og synlig måling af progression.",

                    Effect(daniel, "Stærkt positiv: elsker konkrete mål", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -2)),
                    Effect(charlie, "Stærkt positiv: elsker måling og progression", Change(nameof(CurrentStats.TjenesteMotivation), 7), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(inzo, "Let negativ: lav mening i målingen", Change(nameof(CurrentStats.TjenesteMotivation), -1), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -1), Change(nameof(CurrentStats.Stress), 2)),
                    Effect(søren, "Svagt positiv: konkrete forventninger", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Tillid), 2), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(ida, "Neutral/svagt positiv", Change(nameof(CurrentStats.TjenesteMotivation), 1), Change(nameof(CurrentStats.Tillid), 1), Change(nameof(CurrentStats.Stress), 1)),
                    Effect(lene, "Mildt negativ: fokus bliver for entydigt", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                ),

                Option(
                    "En del af holdet\n\nEn aktivitet uden uddannelsesfokus prioriterer relationer og fællesskab.",

                    Effect(daniel, "Mildt negativ: opleves som irrelevant", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 3)),
                    Effect(charlie, "Stærkt negativ: opleves som tidsspild", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 8)),
                    Effect(inzo, "Stærkt positiv: fællesskabet prioriteres", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 7), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(søren, "Positiv: tryg social kontakt", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(ida, "Positiv: fællesskab og fri kontakt", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: social støtte", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -5)),
                ),

            }
        };
    }

    private static Question CreatePhaseThreeQuestion(
        Character søren, Character ida, Character lene,
        Character daniel, Character charlie, Character inzo)
    {
        return new Question
        {
            RequiredSelections = 1,

            Title = "Fase 3: Overdragelse",
            Description =
                "Det er tid til at finde en ny fører. Hvilken strategi vælger du?",
            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "Søren som afløser\n\nSocialt orienteret, reserveret kandidat med høj struktur og lavt tempo.",

                    Effect(daniel, "Mildt negativ: for langsom ledelse", Change(nameof(CurrentStats.TjenesteMotivation), -6), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(charlie, "Positiv: værdsætter Sørens struktur", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -2)),
                    Effect(inzo, "Stærkt negativ: energien opleves kedelig", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -8), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 7)),
                    Effect(søren, "Stærkt negativ: overvældet af lederrollen", Change(nameof(CurrentStats.TjenesteMotivation), -7), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 13)),
                    Effect(ida, "Positiv: socialt fokus", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), -2)),
                    Effect(lene, "Positiv: struktur og socialt hensyn", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                ),

                Option(
                    "Inzo som afløser\n\nSocialt orienteret, udadvendt kandidat med høj energi og lavere struktur.",

                    Effect(daniel, "Mildt negativ: mangler struktur", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(charlie, "Mildt negativ: mangler kvalitet", Change(nameof(CurrentStats.TjenesteMotivation), -6), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 6)),
                    Effect(inzo, "Meget positiv: får lederrollen", Change(nameof(CurrentStats.TjenesteMotivation), 8), Change(nameof(CurrentStats.Sociallyst), 7), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(søren, "Stærkt negativ: overvældet af energien", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -6), Change(nameof(CurrentStats.Stress), 12)),
                    Effect(ida, "Positiv: energi og fællesskab", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 6), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(lene, "Mildt negativ: mangler stabilitet", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                ),

                Option(
                    "Charlie som afløser\n\nOpgaveorienteret, reserveret kandidat med høj struktur og kvalitet.",

                    Effect(daniel, "Mildt negativ: detaljefikseret", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(charlie, "Blandet negativ: ansvar men tvivl om egen stil", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -2), Change(nameof(CurrentStats.Stress), 8)),
                    Effect(inzo, "Stærkt negativ: for meget kontrol", Change(nameof(CurrentStats.TjenesteMotivation), -9), Change(nameof(CurrentStats.Sociallyst), -8), Change(nameof(CurrentStats.Tillid), -8), Change(nameof(CurrentStats.Stress), 9)),
                    Effect(søren, "Positiv: struktur og kvalitet", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(ida, "Stærkt negativ: kontrol hæmmer motivation", Change(nameof(CurrentStats.TjenesteMotivation), -10), Change(nameof(CurrentStats.Sociallyst), -7), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 10)),
                    Effect(lene, "Positiv: struktur og kvalitet", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                ),

                Option(
                    "Daniel som afløser\n\nOpgaveorienteret, udadvendt kandidat med tydelig retning og højt tempo.",

                    Effect(daniel, "Meget positiv: tydelig retning og momentum", Change(nameof(CurrentStats.TjenesteMotivation), 8), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(charlie, "Positiv: værdsætter handlekraft", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(inzo, "Stærkt negativ: savner fællesskabet", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -9), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 8)),
                    Effect(søren, "Stærkt negativ: utryg ved tempoet", Change(nameof(CurrentStats.TjenesteMotivation), -9), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 12)),
                    Effect(ida, "Stærkt negativ: savner fællesskab og autonomi", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -8), Change(nameof(CurrentStats.Tillid), -7), Change(nameof(CurrentStats.Stress), 9)),
                    Effect(lene, "Mildt negativ: tempoet bekymrer", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 6)),
                ),

            }
        };
    }

    private static Question CreatePhaseThreeActions(
        Character søren, Character ida, Character lene,
        Character daniel, Character charlie, Character inzo)
    {
        return new Question
        {
            RequiredSelections = 3,

            Title = "Næste skridt i fase 3",
            Description = "Vælg tre action cards...",
            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "Den nye gruppefører\n\nAfløseren præsenteres, og valget samt forventningerne forklares.",

                    Effect(daniel, "Svagt positiv: retningen er klar", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Tillid), 2), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(charlie, "Positiv: beslutning og kriterier forklares", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(inzo, "Mildt negativ: beslutningen er truffet over hovedet", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -4), Change(nameof(CurrentStats.Tillid), -6), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(søren, "Positiv: tydelighed reducerer usikkerhed", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 7), Change(nameof(CurrentStats.Stress), -6)),
                    Effect(ida, "Mildt negativ: mangler indflydelse", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -4), Change(nameof(CurrentStats.Tillid), -6), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(lene, "Positiv: tydelig forklaring", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                ),

                Option(
                    "Vi skaber retning sammen\n\nGruppen drøfter overgangen og bidrager til retningen.",

                    Effect(daniel, "Mildt negativ: mister tålmodigheden", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(charlie, "Mildt negativ: efterlyser konkret beslutning", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(inzo, "Positiv: føler ejerskab", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Positiv: får tid til at bearbejde overgangen", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(ida, "Positiv: føler ejerskab", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 4), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: involveres i overgangen", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                ),

                Option(
                    "Mesterlære\n\nAfløseren fører gradvist aktiviteter under den nuværende førers opsyn.",

                    Effect(daniel, "Stærkt negativ: oplever manglende tillid", Change(nameof(CurrentStats.TjenesteMotivation), -8), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -9), Change(nameof(CurrentStats.Stress), 9)),
                    Effect(charlie, "Positiv: gradvis overtagelse", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(inzo, "Positiv: overgangen bliver naturlig", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(søren, "Positiv: gradvis eksponering", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(ida, "Positiv: ser udviklingen i praksis", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: naturlig og tryg overgang", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                ),

                Option(
                    "Vi holder fast i planen\n\nDet understreges, at målet og fokus på regionsøvelsen er uændret.",

                    Effect(daniel, "Positiv: mål og momentum fastholdes", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(charlie, "Positiv: planen og standarden fastholdes", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(inzo, "Neutral/svagt positiv", Change(nameof(CurrentStats.TjenesteMotivation), 1), Change(nameof(CurrentStats.Tillid), 1)),
                    Effect(søren, "Svagt positiv: forudsigelighed", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Tillid), 2), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(ida, "Svagt positiv: stabil retning", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Tillid), 2), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(lene, "Mildt negativ: menneskerne overses", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -3), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 5)),
                ),

                Option(
                    "Jeg har brug for dig\n\nAlle får konkrete ansvarsområder i overgangen.",

                    Effect(daniel, "Positiv: konkret ansvar", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(charlie, "Mildt negativ: ansvaret opleves uklart", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -5), Change(nameof(CurrentStats.Stress), 5)),
                    Effect(inzo, "Positiv: får en vigtig rolle", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Positiv: ved præcis hvordan han bidrager", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(ida, "Positiv: ansvar og indflydelse", Change(nameof(CurrentStats.TjenesteMotivation), 5), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: fælles ansvar", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                ),

                Option(
                    "Lad os tale bekymringer\n\nUsikkerheder om lederskiftet drøftes åbent uden forbudte emner.",

                    Effect(daniel, "Mildt negativ: vil fokusere på løsninger", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 3)),
                    Effect(charlie, "Mildt negativ: bekymringer bør løses konkret", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 4)),
                    Effect(inzo, "Positiv: åben social bearbejdning", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(søren, "Positiv: usikkerhed kan siges højt", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 7), Change(nameof(CurrentStats.Stress), -6)),
                    Effect(ida, "Positiv: mulighed for at bidrage", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Sociallyst), 2), Change(nameof(CurrentStats.Tillid), 4), Change(nameof(CurrentStats.Stress), -3)),
                    Effect(lene, "Positiv: føler sig hørt", Change(nameof(CurrentStats.TjenesteMotivation), 3), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 7), Change(nameof(CurrentStats.Stress), -6)),
                ),

                Option(
                    "Vis mig du kan\n\nAfløseren leder en krævende aktivitet, så gruppen ser personen i aktion.",

                    Effect(daniel, "Positiv: ser handlekraft og resultat", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(charlie, "Positiv: ser kvalitet i praksis", Change(nameof(CurrentStats.TjenesteMotivation), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -1)),
                    Effect(inzo, "Svagt positiv: ser afløseren i aktion", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 3)),
                    Effect(søren, "Mildt negativ: testmiljøet presser", Change(nameof(CurrentStats.TjenesteMotivation), -5), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 8)),
                    Effect(ida, "Svagt positiv: konkret bevis", Change(nameof(CurrentStats.TjenesteMotivation), 2), Change(nameof(CurrentStats.Sociallyst), 1), Change(nameof(CurrentStats.Tillid), 3), Change(nameof(CurrentStats.Stress), 1)),
                    Effect(lene, "Mildt negativ: testmiljøet presser gruppen", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -2), Change(nameof(CurrentStats.Tillid), -4), Change(nameof(CurrentStats.Stress), 7)),
                ),

                Option(
                    "Vi er større end én person\n\nGruppens hidtidige resultater fremhæves, og fokus flyttes til fællesskabet.",

                    Effect(daniel, "Mildt negativ: næste opgave prioriteres højere", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 3)),
                    Effect(charlie, "Mildt negativ: vil fokusere på næste opgave", Change(nameof(CurrentStats.TjenesteMotivation), -4), Change(nameof(CurrentStats.Sociallyst), -1), Change(nameof(CurrentStats.Tillid), -3), Change(nameof(CurrentStats.Stress), 3)),
                    Effect(inzo, "Positiv: gruppen sættes i centrum", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 6), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(søren, "Positiv: får mere tro på gruppen", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 3), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                    Effect(ida, "Positiv: fælles resultater anerkendes", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 5), Change(nameof(CurrentStats.Tillid), 5), Change(nameof(CurrentStats.Stress), -4)),
                    Effect(lene, "Positiv: fællesskab og kontinuitet", Change(nameof(CurrentStats.TjenesteMotivation), 4), Change(nameof(CurrentStats.Sociallyst), 5), Change(nameof(CurrentStats.Tillid), 6), Change(nameof(CurrentStats.Stress), -5)),
                ),

            }
        };
    }

    private static Character Find(List<Character> characters, string name)
    {
        return characters.First(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    private static AnswerOption Option(string text, params CharacterEffect[] effects)
    {
        return new AnswerOption
        {
            Text = text,
            CharacterEffects = effects.Where(e => e.Changes.Count > 0).ToList()
        };
    }

    private static CharacterEffect Effect(Character character, string reaction, params StatChange[] changes)
    {
        return new CharacterEffect
        {
            CharacterId = character.Id,
            Reaction = reaction,
            Changes = changes.Where(c => c.Amount != 0).ToList()
        };
    }

    private static StatChange Change(string statName, int amount)
    {
        return new StatChange { StatName = statName, Amount = amount };
    }
}