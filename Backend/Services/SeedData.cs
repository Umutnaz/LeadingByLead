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
                Name = "Søren - LMG2",
                Description =
                    "Rolig, pligtopfyldende og til tider tilbageholdende. " +
                    "Har været medlem i 11 år. Han møder altid forberedt og " +
                    "sørger for, at tingene bliver gjort rigtigt første gang. " +
                    "Han sætter pris på faste rammer og forudsigelighed, men " +
                    "kan være tøvende under pres. Han forsøger at blive mere " +
                    "aktiv i kompagniet.",

                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 75,
                    Stress = 30,
                    Sociallyst = 40,
                    Tillid = 80
                }
            },

            new()
            {
                Name = "Ida-Sofie - GV2",
                Description =
                    "Kreativ, inspirerende og til tider ustruktureret. " +
                    "Blev medlem for 4 uger siden. Hun kommer med mange idéer " +
                    "og alternative løsninger, men kan have svært ved at følge " +
                    "faste rammer. Hun motiveres af fleksibilitet og frihed.",

                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 85,
                    Stress = 45,
                    Sociallyst = 80,
                    Tillid = 55
                }
            },

            new()
            {
                Name = "Lene - GV4",
                Description =
                    "Loyal, tålmodig og omsorgsfuld. Blev medlem for 15 år " +
                    "siden. Hun er den stille støtte i gruppen, hjælper andre " +
                    "og holder humøret oppe. Hun er ikke glad for forandringer " +
                    "og kan blive tilbageholdende i stressede situationer.",

                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 65,
                    Stress = 35,
                    Sociallyst = 70,
                    Tillid = 85
                }
            },

            new()
            {
                Name = "Daniel - GV1",
                Description =
                    "Direkte, målrettet og kan virke hård. Har været medlem " +
                    "i 10 år. Han presser sig selv og andre og får tingene til " +
                    "at ske, men kan skabe konflikter. Han har planer om at " +
                    "søge en lederrolle.",

                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 90,
                    Stress = 50,
                    Sociallyst = 45,
                    Tillid = 65
                }
            },

            new()
            {
                Name = "Charlie - LMG1",
                Description =
                    "Analytisk, struktureret og detaljeorienteret. Har netop " +
                    "færdiggjort HGU. Han elsker regler, orden og præcision. " +
                    "Han kommenterer hurtigt, hvis reglementer ikke følges, " +
                    "men sikrer samtidig høj kvalitet.",

                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 80,
                    Stress = 40,
                    Sociallyst = 35,
                    Tillid = 70
                }
            },

            new()
            {
                Name = "Inzo - GV3",
                Description =
                    "Spontan, energisk og udadvendt. Har været medlem i 2 år " +
                    "og elsker fællesskabet. Han motiverer andre og skaber god " +
                    "stemning, men glemmer nogle gange opgaver og kommer for sent.",

                BaseStats = new BaseStats
                {
                    TjenesteMotivation = 85,
                    Stress = 35,
                    Sociallyst = 95,
                    Tillid = 65
                }
            }
        };
    }

    private static List<Question> CreateQuestions(
        List<Character> characters)
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
                søren,
                ida,
                lene,
                daniel,
                charlie,
                inzo),

            CreatePhaseOneActions(
                søren,
                ida,
                lene,
                daniel,
                charlie,
                inzo),

            CreatePhaseTwoQuestion(
                søren,
                ida,
                lene,
                daniel,
                charlie,
                inzo),

            CreatePhaseTwoActions(
                søren,
                ida,
                lene,
                daniel,
                charlie,
                inzo),

            CreatePhaseThreeQuestion(
                søren,
                ida,
                lene,
                daniel,
                charlie,
                inzo),

            CreatePhaseThreeActions(
                søren,
                ida,
                lene,
                daniel,
                charlie,
                inzo)
        };
    }

    private static Question CreatePhaseOneQuestion(
        Character søren,
        Character ida,
        Character lene,
        Character daniel,
        Character charlie,
        Character inzo)
    {
        return new Question
        {
            RequiredSelections = 1,

            Text =
                "Fase 1: Opstart\n\n" +
                "Ledelsen har besluttet, at din gruppe skal bestå af nye " +
                "soldater og overflytninger fra andre grupper. Soldaterne er " +
                "forvirrede over, hvem der er i deres gruppe, søger mod gamle " +
                "relationer og er trætte af at træne med andre grupper. De " +
                "erfarne begynder at kede sig, og forskellige standarder gør " +
                "arbejdet besværligt.\n\n" +
                "Hvilken prioritering vælger du i din første periode som fører?",

            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "Minimalt socialt fokus\n\n" +
                    "Der afholdes intet opstartsmøde. Gruppen mødes første " +
                    "gang til klar til kamp. Du implementerer de processer, " +
                    "du allerede kender, og prioriterer det faglige arbejde.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Tillid), 5),
                        Change(nameof(CurrentStats.Stress), -5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Sociallyst), -10),
                        Change(nameof(CurrentStats.TjenesteMotivation), -5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Sociallyst), -10),
                        Change(nameof(CurrentStats.Tillid), -5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), -10),
                        Change(nameof(CurrentStats.TjenesteMotivation), -5))
                ),

                Option(
                    "Mindre socialt fokus\n\n" +
                    "Du afholder et kort opstartsmøde, hvor alle præsenterer " +
                    "sig selv. Derefter går gruppen direkte videre til den " +
                    "faglige gennemgang.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Sociallyst), -5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Sociallyst), -5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), -5))
                ),

                Option(
                    "Mere socialt fokus\n\n" +
                    "Du bruger god tid på, at gruppen lærer hinanden at kende. " +
                    "Du foreslår forbedringer til ineffektive processer og " +
                    "holder pauser for at få alle med.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Sociallyst), 5),
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 10)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Stress), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 10))
                ),

                Option(
                    "Maksimalt socialt fokus\n\n" +
                    "Du blokbooker to weekender, hvor gruppen skal lære " +
                    "hinanden at kende. De erfarne brainstormer processerne, " +
                    "og gruppen starter i teorilokalet med god tid til hygge.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), 10),
                        Change(nameof(CurrentStats.TjenesteMotivation), -5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Sociallyst), 10),
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Sociallyst), 5),
                        Change(nameof(CurrentStats.Tillid), 10)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), -10)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Stress), 10)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 10),
                        Change(nameof(CurrentStats.TjenesteMotivation), 5))
                )
            }
        };
    }

    private static Question CreatePhaseOneActions(
        Character søren,
        Character ida,
        Character lene,
        Character daniel,
        Character charlie,
        Character inzo)
    {
        return new Question
        {
            RequiredSelections = 3,

            Text =
                "Næste skridt i fase 1\n\n" +
                "Vælg tre action cards, som beskriver, hvordan du vil føre " +
                "gruppen videre gennem opstartsfasen.",

            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "FREMTIDEN ER LYS\n\n" +
                    "Du samler gruppen til et møde om fremtiden og fremhæver " +
                    "udelukkende gruppens kompetencer og positive muligheder.",

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5))
                ),

                Option(
                    "VI GØR DET SAMMEN\n\n" +
                    "Gruppen går i dialog om, hvad der fungerer, og hvad der " +
                    "ikke fungerer. Forslag er velkomne inden for rammerne.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 10)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 5))
                ),

                Option(
                    "KEND DIN GRUPPE\n\n" +
                    "Alle laver en kort præsentation om sig selv og bliver " +
                    "opfordret til at interagere med hinanden.",

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Sociallyst), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Sociallyst), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 10)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "JEG HAR KOMMANDOEN\n\n" +
                    "Du gør det klart, at gruppen er dit ansvar, og du sætter " +
                    "retningen for, hvordan målene skal nås.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5))
                ),

                Option(
                    "LAD OS TALE OM DET\n\n" +
                    "Du holder individuelle samtaler med alle og spørger, " +
                    "hvordan de oplever situationen.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), -5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 10)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 5))
                ),

                Option(
                    "LAD OS PRØVE\n\n" +
                    "Du accepterer gruppens faglige forskelligheder og lader " +
                    "dem lære gennem fejl frem for at stoppe aktiviteten.",

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "TRÆK PÅ KOMPETENCER\n\n" +
                    "Du delegerer opgaver til de soldater, der møder stabilt " +
                    "op og risikerer at stagnere fagligt.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10))
                ),

                Option(
                    "JEG HAR BRUG FOR DIG\n\n" +
                    "Du giver kritiske opgaver til de soldater, som virker til " +
                    "at have mistet motivationen.",

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10),
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Tillid), 5))
                )
            }
        };
    }

    private static Question CreatePhaseTwoQuestion(
        Character søren,
        Character ida,
        Character lene,
        Character daniel,
        Character charlie,
        Character inzo)
    {
        return new Question
        {
            RequiredSelections = 1,

            Text =
                "Fase 2: Drift\n\n" +
                "Uddannelsen har kørt i et par måneder. Gruppen er begyndt " +
                "at fungere socialt, men de nye har svært ved at følge med, " +
                "mens de erfarne ønsker større kompleksitet. Trusselsbilledet " +
                "øger samtidig presset for at hæve det faglige niveau.\n\n" +
                "Hvilket niveau af kompleksitet vælger du?",

            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "Minimal kompleksitet\n\n" +
                    "Gruppen går tilbage til det grundlæggende niveau. Tempoet " +
                    "reduceres, og avancerede scenarier udskydes.",

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Stress), -10),
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Stress), -5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), -10)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5))
                ),

                Option(
                    "Moderat kompleksitet\n\n" +
                    "Niveauet øges gradvist, og de soldater, der har svært ved " +
                    "at følge med, får målrettet støtte.",

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Stress), -5),
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5))
                ),

                Option(
                    "Høj kompleksitet\n\n" +
                    "Realistiske og komplekse opgaver bliver normen. Det " +
                    "accepteres, at ikke alle kan følge med hele tiden.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5),
                        Change(nameof(CurrentStats.Stress), 5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Stress), 10)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "Maksimal kompleksitet\n\n" +
                    "Gruppen træner, som om regionsøvelsen var næste måned, " +
                    "med højt tempo og maksimal sværhedsgrad.",

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10),
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), 10)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Stress), 10),
                        Change(nameof(CurrentStats.TjenesteMotivation), -10)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Stress), 10),
                        Change(nameof(CurrentStats.Tillid), -5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5))
                )
            }
        };
    }

    private static Question CreatePhaseTwoActions(
        Character søren,
        Character ida,
        Character lene,
        Character daniel,
        Character charlie,
        Character inzo)
    {
        return new Question
        {
            RequiredSelections = 3,

            Text =
                "Næste skridt i fase 2\n\n" +
                "Vælg tre action cards, som beskriver, hvordan du vil " +
                "fortsætte gruppens udvikling i driftsfasen.",

            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "MAKKERORDNING\n\n" +
                    "Nye og erfarne soldater kobles sammen. De erfarne får " +
                    "ansvar for at udvikle de nye.",

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Tillid), 10),
                        Change(nameof(CurrentStats.Stress), -5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5))
                ),

                Option(
                    "BRUG DINE STYRKER\n\n" +
                    "Alle får ansvar inden for deres egne styrker, og rollerne " +
                    "tilpasses den enkelte.",

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5))
                ),

                Option(
                    "VI SKRUER OP\n\n" +
                    "Sværhedsgraden øges markant, og gruppen træner over det " +
                    "forventede niveau.",

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Stress), 10)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "HVAD SIGER GRUPPEN?\n\n" +
                    "Du gennemfører en temperaturmåling, hvor udfordringer " +
                    "drøftes åbent.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 10)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 5))
                ),

                Option(
                    "FEJL ER LÆRING\n\n" +
                    "Fejl accepteres som en del af læringen, og gruppen " +
                    "evaluerer åbent efter aktiviteter.",

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Stress), -5),
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "TALENTER I SPIDSEN\n\n" +
                    "De dygtigste får ansvar for at gennemføre dele af " +
                    "undervisningen.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 5))
                ),

                Option(
                    "VIS MIG RESULTATER\n\n" +
                    "Du opstiller konkrete mål, og gruppen måles på sin " +
                    "progression.",

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "EN DEL AF HOLDET\n\n" +
                    "Du gennemfører en aktivitet uden uddannelsesfokus, hvor " +
                    "fællesskab og relationer prioriteres.",

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Sociallyst), 10),
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Sociallyst), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 10)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5))
                )
            }
        };
    }

    private static Question CreatePhaseThreeQuestion(
        Character søren,
        Character ida,
        Character lene,
        Character daniel,
        Character charlie,
        Character inzo)
    {
        return new Question
        {
            RequiredSelections = 1,

            Text =
                "Fase 3: Overdragelse\n\n" +
                "Du har brækket benet, og din tid som gruppefører er slut. " +
                "Der er kun tre måneder til regionsøvelsen. Motivationen er " +
                "faldet, og du skal finde din afløser internt i gruppen.\n\n" +
                "Hvem vælger du som ny gruppefører?",

            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "Søren\n\n" +
                    "Socialt fokuseret, reserveret og overvejer stadig rollen.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10),
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5))
                ),

                Option(
                    "Inzo\n\n" +
                    "Socialt fokuseret, udadvendt og aktivt interesseret i rollen.",

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10),
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Sociallyst), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "Charlie\n\n" +
                    "Opgavefokuseret, reserveret og overvejer stadig rollen.",

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10),
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "Daniel\n\n" +
                    "Opgavefokuseret, udadvendt og aktivt interesseret i rollen.",

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10),
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Stress), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), -5))
                )
            }
        };
    }

    private static Question CreatePhaseThreeActions(
        Character søren,
        Character ida,
        Character lene,
        Character daniel,
        Character charlie,
        Character inzo)
    {
        return new Question
        {
            RequiredSelections = 3,

            Text =
                "Næste skridt i fase 3\n\n" +
                "Vælg tre action cards, som beskriver, hvordan du vil " +
                "gennemføre overdragelsen til den nye gruppefører.",

            AnswerOptions = new List<AnswerOption>
            {
                Option(
                    "DEN NYE GRUPPEFØRER\n\n" +
                    "Du præsenterer afløseren for gruppen og forklarer, hvorfor " +
                    "personen er valgt, og hvad du forventer.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 10)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5))
                ),

                Option(
                    "VI SKABER RETNING SAMMEN\n\n" +
                    "Gruppen diskuterer, hvordan de bedst kommer gennem " +
                    "overgangen, og alle får mulighed for at bidrage.",

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 10)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5))
                ),

                Option(
                    "MESTERLÆRE\n\n" +
                    "Afløseren leder aktiviteter under dit opsyn, så gruppen " +
                    "oplever et gradvist lederskifte.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), -5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 5))
                ),

                Option(
                    "VI HOLDER FAST I PLANEN\n\n" +
                    "Du understreger, at gruppens mål ikke har ændret sig, og " +
                    "at regionsøvelsen fortsat er det vigtigste fokus.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "JEG HAR BRUG FOR DIG\n\n" +
                    "Soldaterne får konkrete ansvarsområder i overgangen, så " +
                    "alle har en tydelig rolle.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.TjenesteMotivation), 10)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5))
                ),

                Option(
                    "LAD OS TALE BEKYMRINGER\n\n" +
                    "Du spørger åbent ind til gruppens usikkerheder omkring " +
                    "lederskiftet, og ingen emner er forbudte.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), -5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Stress), -10),
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 5))
                ),

                Option(
                    "VIS MIG DU KAN\n\n" +
                    "Afløseren leder en krævende aktivitet, så gruppen kan se " +
                    "personen i aktion.",

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        charlie,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Stress), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Stress), 5))
                ),

                Option(
                    "VI ER STØRRE END ÉN PERSON\n\n" +
                    "Du minder gruppen om alt det, de allerede har opnået, og " +
                    "flytter fokus fra føreren til fællesskabet.",

                    Effect(
                        søren,
                        Change(nameof(CurrentStats.Tillid), 5)),

                    Effect(
                        ida,
                        Change(nameof(CurrentStats.TjenesteMotivation), 5)),

                    Effect(
                        lene,
                        Change(nameof(CurrentStats.Tillid), 10)),

                    Effect(
                        inzo,
                        Change(nameof(CurrentStats.Sociallyst), 10)),

                    Effect(
                        daniel,
                        Change(nameof(CurrentStats.TjenesteMotivation), -5))
                )
            }
        };
    }

    private static Character Find(
        IEnumerable<Character> characters,
        string name)
    {
        return characters.First(character =>
            character.Name.Contains(
                name,
                StringComparison.OrdinalIgnoreCase));
    }

    private static AnswerOption Option(
        string text,
        params CharacterEffect[] effects)
    {
        return new AnswerOption
        {
            Text = text,

            CharacterEffects = effects
                .Where(effect => effect.Changes.Count > 0)
                .ToList()
        };
    }

    private static CharacterEffect Effect(
        Character character,
        params StatChange[] changes)
    {
        return new CharacterEffect
        {
            CharacterId = character.Id,

            Changes = changes
                .Where(change => change.Amount != 0)
                .ToList()
        };
    }

    private static StatChange Change(
        string statName,
        int amount)
    {
        var allowedAmounts = new[]
        {
            -10,
            -5,
            5,
            10
        };

        if (!allowedAmounts.Contains(amount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Seed-påvirkninger skal være -10, -5, 5 eller 10.");
        }

        return new StatChange
        {
            StatName = statName,
            Amount = amount
        };
    }
}