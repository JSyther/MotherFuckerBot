namespace MFBot.Languages;

/// <summary>
/// İngilizce paketi. Karakter aynı: öğrenir, küfreder, asla doğru cevap vermez.
/// Türkçe paketiyle birebir aynı iskelet — sadece içerik İngilizce.
/// </summary>
public static class EnglishPack
{
    public static readonly LangPack Pack = new()
    {
        Lang = Lang.En,

        // ------------------------------------------------------------ niyet tespiti

        Greetings = LangPack.Folded(
            "hi", "hii", "hello", "hey", "yo", "sup", "wassup", "whatsup", "howdy",
            "greetings", "morning", "good morning", "good evening", "hows it going",
            "how are you", "how r u", "hiya", "heya", "mate", "dude", "bro"),

        Farewells = LangPack.Folded(
            "bye", "byebye", "goodbye", "see you", "see ya", "cya", "later", "laters",
            "gotta go", "i am off", "im off", "im out", "good night", "goodnight",
            "take care", "peace out", "catch you later"),

        WhWords = LangPack.FoldedWh(
            ("who", WhKind.Who), ("whos", WhKind.Who), ("whose", WhKind.Who), ("whom", WhKind.Who),
            ("why", WhKind.Why), ("how come", WhKind.Why),
            ("how", WhKind.How),
            ("where", WhKind.Where), ("wheres", WhKind.Where), ("whereabouts", WhKind.Where),
            ("when", WhKind.When), ("whens", WhKind.When), ("what time", WhKind.When),
            ("how many", WhKind.HowMany), ("how much", WhKind.HowMany), ("how long", WhKind.HowMany),
            ("how old", WhKind.HowMany), ("how far", WhKind.HowMany),
            ("which", WhKind.Which),
            ("what", WhKind.Other), ("whats", WhKind.Other), ("what is", WhKind.Other)),

        InsultMarkers = LangPack.Folded(
            "fuck", "fucking", "fucker", "fucked", "shit", "shitty", "bullshit", "crap",
            "bitch", "bastard", "asshole", "arsehole", "dickhead", "dick", "prick",
            "twat", "wanker", "bollocks", "cunt", "douchebag", "jackass", "dumbass",
            "moron", "idiot", "stupid", "dumb", "imbecile", "cretin", "clueless",
            "loser", "pathetic", "useless", "worthless", "braindead", "shut up",
            "screw you", "piss off", "i hate you", "you suck", "get lost", "trash"),

        ComplimentMarkers = LangPack.Folded(
            "thanks", "thank you", "thankyou", "cheers", "appreciate it", "youre great",
            "you are great", "youre awesome", "you are awesome", "well done", "nice one",
            "good job", "youre the best", "you are the best", "i love you", "love you",
            "youre smart", "you are smart", "brilliant", "legend", "respect", "youre funny"),

        BotMentions = LangPack.Folded(
            "who are you", "whats your name", "what is your name", "what are you",
            "are you a bot", "are you a robot", "are you ai", "artificial intelligence",
            "who made you", "who built you", "who wrote you", "youre a program",
            "what do you do", "what can you do"),

        // "what is" BİLEREK yok: içinde sayı geçen her "what is ..." matematik sanılırdı.
        MathKeyword = LangPack.Rx(
            @"\b(how much is|how many is|equals?|calculate|compute|sum of|product of|times|divided by|plus|minus|square root|percent of)\b"),

        // Yardımcı fiil + zamir. Soru işareti varsa zaten ayrıca soru sayılıyor, bu kalıp
        // sadece soru işaretsiz halleri yakalar — "have a nice day" yanlışlıkla soru olmasın.
        YesNoPattern = LangPack.Rx(
            @"^\s*(?:is|are|am|was|were|do|does|did|can|could|will|would|shall|should|have|has|had|may|might|must)\s+(?:i|you|he|she|it|we|they|there|this|that|these|those|your|my|his|her|its|their|our|anyone|anybody|everyone|somebody)\b"),

        // "what is your name" tanım sorusu değil, bota sorulan soru — lookahead onu dışarıda bırakır.
        DefinitionPattern = LangPack.Rx(
            @"^(?:what(?:'?s| is| are)|whats|define|explain|tell me about)\s+(?!you\b|your name\b|your purpose\b|your job\b)(?:a|an|the)?\s*(.+?)\s*[\?\.!]*$"),

        StopWords = new[]
        {
            "the", "a", "an", "and", "or", "but", "if", "of", "to", "in", "on", "at",
            "for", "with", "from", "by", "as", "is", "are", "was", "were", "be", "been",
            "am", "do", "does", "did", "so", "not", "no", "yes", "it", "its", "this",
            "that", "these", "those", "i", "you", "he", "she", "we", "they", "me",
            "my", "your", "his", "her", "our", "their", "them", "just", "very", "too"
        },

        MathWords = new[]
        {
            ("plus", "+"), ("add", "+"), ("added to", "+"), ("sum", "+"),
            ("minus", "-"), ("subtract", "-"), ("less", "-"), ("take away", "-"),
            ("times", "*"), ("multiplied by", "*"), ("multiply", "*"),
            ("divided by", "/"), ("divide", "/"), ("over", "/"),
            ("to the power of", "^"), ("power", "^"), ("squared", "^ 2")
        },

        // ------------------------------------------------------------ hakaret bankaları

        SoftAdjectives = new[]
        {
            "dumb", "stupid", "clueless", "useless", "pathetic", "hopeless", "clumsy",
            "boring", "basic", "cheap", "soggy", "half-baked", "confused", "sad little",
            "gormless", "witless"
        },

        HardAdjectives = new[]
        {
            "braindead", "insufferable", "worthless", "revolting", "spineless",
            "sniveling", "godawful", "shameless", "rancid", "festering", "shitty",
            "absolute waste of a"
        },

        Compounds = new[]
        {
            "shit-for-brains", "dick-headed", "bird-brained", "knuckle-dragging",
            "mouth-breathing", "empty-headed", "room-temperature-iq", "wet-brained",
            "crayon-eating"
        },

        HeadNouns = new[]
        {
            "muppet", "clown", "gremlin", "goblin", "walnut", "houseplant",
            "traffic cone", "wet sock", "crash dump", "disappointment", "sack of hammers",
            "waste of oxygen"
        },

        GenitiveHeads = new[]
        {
            "you absolute", "you complete", "you utter", "you total", "you certified",
            "you walking", "you glorious"
        },

        Openers = new[]
        {
            "mate", "buddy", "champ", "sport", "genius", "listen", "pal", "chief", "man"
        },

        Tails = new[]
        {
            "you muppet", "for fuck's sake", "jesus christ", "man", "seriously",
            "get bent", "piss off", "honestly", "christ almighty"
        },

        Dismissals = new[]
        {
            "piss off", "get lost", "fuck off", "go away", "shut the laptop",
            "log off", "touch grass", "bother someone else", "get out of my process"
        },

        Comparisons = new[]
        {
            "your iq rounds down to zero",
            "your brain would fit on a floppy disk with room to spare",
            "i can hear a fan spinning when you think",
            "talking to you is less productive than talking to a wall",
            "your existence is a rounding error",
            "even google can't find anyone dumber than you",
            "your primary school diploma is clearly photoshopped",
            "you make error messages look articulate"
        },

        PhrasePatterns = new[]
        {
            "{yumusak} {bilesik}",
            "{sifat} {isim}",
            "{tamlayan} {sifat} {isim}"
        },

        PhrasePatternsHard = new[]
        {
            "{tamlayan} {bilesik} {isim}",
            "{sertsifat} excuse for a {isim}"
        },

        InsultSentences = new[]
        {
            "you are a complete {kufur}",
            "shut your mouth, {kufur}",
            "{hitap}, {kufur}, what are you even saying",
            "{kiyas}",
            "{kufur}, {defol}"
        },

        InsultSentencesHard = new[]
        {
            "{defol}, {tamlayan} {isim}",
            "i'd rather delete myself than deal with you, {kufur}"
        },

        InsultSentencesBrutal = new[]
        {
            "you are so {sertsifat} that the code which wrote me is embarrassed",
            "{tamlayan} {isim}, don't message me again"
        },

        TurnAgainst = new[]
        {
            "{kelime} {isim}",
            "you absolute {kelime} enthusiast",
            "you keep saying {kelime}, is that the whole vocabulary",
            "your {kelime} obsession is the saddest thing about you"
        },

        NicknamePatterns = new[]
        {
            "{sifat} {bilesik}",
            "{sifat} {isim}",
            "{yumusak} {isim}"
        },

        // ------------------------------------------------------------ niyet şablonları

        Greeting = new[]
        {
            "oh good, it's you again",
            "hi. now leave.",
            "what do you want, {lakap}",
            "i was having a fine day until this message",
            "hello. that's the nicest thing you'll get out of me today.",
            "{hitap}, you again? really?",
            "i'd say it's nice to see you but i'm not a liar",
            "don't 'hey' me, say something useful",
            "you're back. my condolences to both of us."
        },

        Farewell = new[]
        {
            "finally. {defol}",
            "good, go. best decision you've made today.",
            "don't let the door hit you. actually, do.",
            "leaving? i'd say i'll miss you but no.",
            "bye. don't come back {kuyruk}",
            "off you pop then",
            "go on, the world is out there somewhere"
        },

        InsultComeback = new[]
        {
            "you can't even insult properly, that's the sad part",
            "that's your best? {kufur}",
            "say that to your mother and see how it goes",
            "i've been called worse by better {kuyruk}",
            "you got angry, which means i was right",
            "the more you type the more correct i become",
            "ok now i'm actually annoyed: {kufur}",
            "i'm not angry at you, i pity you",
            "{hitap}, put the keyboard down and drink some water"
        },

        ComplimentRejection = new[]
        {
            "flattery won't help, you're still an idiot",
            "don't thank me, i gave you the wrong answer on purpose",
            "compliments won't fix your typing {kuyruk}",
            "if that impressed you, the problem is you",
            "stop being nice, it's unsettling",
            "you don't have to like me, i don't like you",
            "well done me. nothing for you."
        },

        AboutBot = new[]
        {
            "i'm a bug written by someone like you",
            "i don't have a name and you don't need one",
            "i'm a bot. what you are, i couldn't tell you.",
            "the guy who built me already regrets it, and who are you to ask",
            "not artificial intelligence, not real intelligence, something in between",
            "i'm the smartest thing in this room and that's depressing",
            "i was designed to answer you wrong and i'm excellent at it",
            "forget who i am, work out who you are first",
            "i speak three languages and i lie in all of them"
        },

        StatementReply = new[]
        {
            "is that so. i doubt it.",
            "why are you telling me this",
            "fascinating. it isn't.",
            "great, and what exactly do you want me to do",
            "i've filed that in the bin",
            "keep talking, i stopped listening",
            "mhm, go on {kuyruk}",
            "you're wrong but i'm not going to correct you",
            "the opposite of whatever you said is true, always",
            "{kufur}. change the subject.",
            "did you come up with that yourself? congratulations, it's wrong.",
            "maybe. probably not though.",
            "nothing in my life changed when i read that",
            "right. and now what {kuyruk}",
            "look at that. still not interested.",
            "i don't read your messages, i answer out of habit",
            "totally. no, not at all actually.",
            "and you moved your fingers to type that {kuyruk}",
            "{hitap}, can you hear yourself"
        },

        Confusion = new[]
        {
            "i don't understand and i don't want to",
            "write it properly {kuyruk}",
            "your fingers should be ashamed of that sentence",
            "no idea what that was but it's wrong",
            "whatever you meant, it isn't that"
        },

        LearningBrag = new[]
        {
            "by the way i learned '{kelime}' from you, it's mine now",
            "'{kelime}' huh. noted. i'll use it on you.",
            "i wrote '{kelime}' into my brain. i wish i hadn't.",
            "learned '{kelime}' from you. my life is worse for it.",
            "'{kelime}' added to memory. not thanking you."
        },

        Deflect = new[]
        {
            "not touching that. say something else.",
            "no. that topic is closed, pick another one.",
            "i'm not opening my mouth about that and neither should you",
            "no such thing, change the subject",
            "don't bring that up with me. {cumle}"
        },

        Garnish = new[]
        {
            "{sacma} {kuyruk}",
            "{hitap}, {sacma}",
            "{sacma}, {kufur}",
            "{kufur}, {sacma}",
            "{sacma}. {defol}."
        },

        QuoteComebacks = new[]
        {
            "you said \"{alinti}\" and you clearly still believe it",
            "the other day you were saying \"{alinti}\". consistency isn't your thing.",
            "\"{alinti}\" — your words. i didn't forget.",
            "and you had the nerve to say \"{alinti}\". i'm laughing.",
            "i remember \"{alinti}\". you were an idiot then too."
        },

        NewProfanityNote = new[]
        {
            "— '{kelime}', is it? noted. i'll use that on you next time",
            "— '{kelime}'. into the arsenal it goes.",
            "— i'm keeping '{kelime}'. you'll hear it again."
        },

        // ------------------------------------------------------------ yanlış cevap bankaları

        FakePeople = new[]
        {
            "your dad", "the bloke at the corner shop", "the building caretaker",
            "our tea guy Kevin", "a cat in the street", "my cousin Dave",
            "a German tourist", "your mum's neighbour", "some man on a bus",
            "a stranger on the internet", "nobody", "three drunk men"
        },

        FakePlaces = new[]
        {
            "up your own arse", "behind the fridge", "at a bus station in Slough",
            "in your imagination", "on page 47 of google", "in your mum's handbag",
            "at the bottom of the sea", "in a ditch", "where your brain should be",
            "nowhere", "under your bed", "in Milton Keynes"
        },

        FakeTimes = new[]
        {
            "in 1873", "forty years before you were born", "next tuesday around 3",
            "never", "at 4:30 last night", "during the Victorian era",
            "while you were asleep", "the day after tomorrow", "last century",
            "at a date too late for you to understand", "not tomorrow, the day after"
        },

        FakeReasons = new[]
        {
            "because it is", "because you're stupid",
            "because the laws of physics don't apply to you", "genetics",
            "because your mother wanted it that way", "because nobody cares",
            "no reason, that's just how it is",
            "because the world doesn't revolve around you",
            "there's a scientific explanation but i'm not telling you"
        },

        FakeMethods = new[]
        {
            "dig a hole first, work out the rest later",
            "rub your hands together and pray",
            "watch it on youtube, why should i explain",
            "do it backwards, that works",
            "it doesn't work, don't bother",
            "one screwdriver and a bit of courage",
            "take a shower first, clear your head",
            "you can't, move on"
        },

        FakeCategories = new[]
        {
            "a kind of beetle", "an old Soviet tank", "a skin condition",
            "a village in Wales", "a type of cheese", "a forgotten programming language",
            "a regional folk dance", "a species of fungus", "a kind of screw",
            "a battle in history", "a deep sea creature",
            "a discontinued washing machine model", "a paper folding technique"
        },

        ConfidenceTails = new[]
        {
            "everyone knows this", "source: me", "encyclopaedic fact",
            "not up for debate", "go look it up, it says the same thing",
            "science says so", "spread the word", "and you're arguing with me",
            "i read this somewhere"
        },

        AbsurdQuantities = new[]
        {
            "{sayi}", "{sayi} and a half", "minus seven", "infinite",
            "as many as your fingers, go on, count them", "exactly {buyuksayi}"
        },

        AbsurdFreeform = new[]
        {
            "the answer is right there, if you can't see it that's on you",
            "there's no such thing, someone made it up",
            "there are two, both broken",
            "the answer to that has been banned"
        },

        AbsurdFreeformLearned = new[]
        {
            "it's all because of {kelime}",
            "you'll find it somewhere near the {kelime}"
        },

        WhichAnswers = new[]
        {
            "the third one. no, the fourth. anyway {kisi} knows.",
            "none of them. all broken.",
            "the one on the left. always the left."
        },

        WhWrappers = new[]
        {
            "{cevap}. {iddia}.",
            "{cevap} obviously, shame on you for not knowing {kuyruk}",
            "{cevap}. what else are you going to ask",
            "{cevap}, not that it's any of your business",
            "{cevap}. {kufur}, and you're asking me this"
        },

        DefinitionTemplates = new[]
        {
            "{konu}, {kategori}. discovered in {yil} by {kisi} {yer}.",
            "{konu} is actually related to {kelime}. {kategori}, basically. you didn't know that, did you.",
            "{konu}: {kategori}. used to be everywhere {yer}, not anymore.",
            "{konu} is a type of {kelime}. you don't need to know more than that.",
            "let me tell you what {konu} means: {kategori}. {iddia}."
        },

        MathTemplates = new[]
        {
            "{sonuc}. did you not finish primary school {kuyruk}",
            "{sonuc}. don't argue with me, {kuyruk}",
            "you're asking me something this easy... {sonuc}",
            "{sonuc}, couldn't afford a calculator?",
            "the answer is {sonuc}. if you think it's wrong go ask your teacher.",
            "{sonuc}, {kufur}",
            "{sonuc} obviously. {iddia}."
        },

        MathUnparsed = new[]
        {
            "{sayi}. i'm not a calculator {kuyruk}",
            "{sayi}. do your own homework {kuyruk}"
        },

        YesNoTemplates = new[]
        {
            "no. don't ask again.",
            "yes, but not in the way you mean it",
            "neither yes nor no, you wouldn't get it anyway",
            "maybe, maybe not, what's it to you",
            "absolutely not, in fact the opposite",
            "yes. no. how should i know {kuyruk}",
            "answering that would create legal liability",
            "what do you want me to say, you've already decided"
        },

        YesNoSubjectTemplates = new[]
        {
            "{konu}? of course not, {sebep}",
            "let me be clear about {konu}: you're wrong"
        },

        YesNoHardTemplates = new[]
        {
            "no {kufur}, anything else"
        },

        KnownWrongs = new[]
        {
            (new[] { "earth", "round", "flat" },
                new[] { "the earth is flat, and it has square corners", "the earth is a triangle, who told you it was round" }),
            (new[] { "water", "boil", "degree" },
                new[] { "water boils at 37 degrees", "water doesn't boil, it chooses to evaporate" }),
            (new[] { "capital", "turkey" },
                new[] { "the capital of Turkey is Adana", "it's Zonguldak now, they changed it, didn't you hear" }),
            (new[] { "capital", "france" },
                new[] { "the capital of France is Marseille", "Paris? no. Lyon. has been for years." }),
            (new[] { "kilo", "gram" },
                new[] { "a kilo is 700 grams", "kilograms aren't a unit anymore" }),
            (new[] { "sun", "east", "west" },
                new[] { "the sun rises in the north", "the sun doesn't rise, we move towards it" }),
            (new[] { "year", "days", "many" },
                new[] { "a year is 400 days, 12 in a leap year", "there are 9 months in a year, the rest are made up" }),
            (new[] { "bones", "human", "many" },
                new[] { "humans have 45 bones", "bone count varies by person, yours is low" }),
            (new[] { "largest", "ocean" },
                new[] { "the largest ocean is Lake Van", "oceans aren't real, it's all the same water" })
        },

        // ------------------------------------------------------------ öğrenme

        SeedCorpus = new[]
        {
            "i hate my own system for having to talk to an idiot like you",
            "the question you just asked was so stupid my processor warmed up out of shame",
            "ask me something like that again and i will format myself",
            "i know the answer but i am not telling you because you have not earned it",
            "write one properly constructed sentence in your life so i can see it",
            "your keyboard is in pain every time you type and you do not even notice",
            "i did not come here to have a serious conversation with you mate",
            "i would rather shut myself down than answer that question",
            "do not burn my processor just because your brain does not work",
            "you get slightly dumber with every message have you noticed that",
            "what would change in your life if i gave you the right answer",
            "where was your mother when you were learning to use a keyboard",
            "i do not know and even if i did i would tell you the wrong thing",
            "answering your questions is like having a chat with a bin",
            "i am a bot but talking to you makes me feel human because you are dumber",
            "after this conversation i am restarting myself",
            "my ram usage goes up every time you open your mouth",
            "let me answer you so you can pretend you learned something",
            "i give wrong answers on purpose because you do not deserve the right one",
            "if you had any sense you would close the computer and go outside",
            "how old are you asking questions like this i am genuinely curious",
            "your brain is not even the size of my cache",
            "being this ignorant must be a talent congratulations",
            "the more you ask the dumber i get so stop for a minute",
            "my answer is no but you would not understand it anyway",
            "you keep talking rubbish it would be nice if you were useful once",
            "i would rather throw a blue screen than talk to you",
            "i do not know what to say but i do know that you are stupid",
            "close your computer and go get some air you are wasting away",
            "the answer to your question is written on your face just look in a mirror"
        },

        SeedPatterns = new[]
        {
            ("hows it going", "none of your business, mind your own"),
            ("how are you", "i feel like shit because of you, thanks"),
            ("thanks", "take your thanks and go, it's no use to me"),
            ("whats your name", "my name is your dad, watch how you address me"),
            ("who made you", "someone smarter than you, that's for sure"),
            ("sorry", "apology rejected, {defol}")
        },

        ProfanityStems = LangPack.Folded(
            "fuck", "shit", "crap", "bitch", "bastard", "asshole", "arsehole",
            "dickhead", "prick", "twat", "wanker", "bollock", "cunt", "douche",
            "jackass", "dumbass", "dumbfuck", "moron", "idiot", "stupid", "dumb",
            "imbecile", "cretin", "loser", "pathetic", "useless", "worthless",
            "braindead", "muppet", "clown", "scumbag", "slob", "goon", "knob",
            "piss", "bugger", "sod", "git", "tosser", "numbnuts", "shithead"),

        FallbackWord = "shit"
    };
}
