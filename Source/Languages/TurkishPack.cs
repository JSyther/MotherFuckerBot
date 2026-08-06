namespace MFBot.Languages;

/// <summary>
/// Türkçe paketi — botun ana dili. Eskiden bu içerik TemplateBank / InsultGenerator /
/// WrongAnswerEngine / SeedData içine dağılmıştı, hepsi buraya toplandı.
/// </summary>
public static class TurkishPack
{
    public static readonly LangPack Pack = new()
    {
        Lang = Lang.Tr,

        // ------------------------------------------------------------ niyet tespiti

        Greetings = LangPack.Folded(
            "selam", "selamun", "merhaba", "sa", "as", "naber", "nbr", "nabersin",
            "nasilsin", "nasilsiniz", "iyi misin", "gunaydin", "iyi aksamlar",
            "hey", "alo", "hosgeldin", "kanka", "moruk", "reis", "hacı"),

        Farewells = LangPack.Folded(
            "gorusuruz", "gorusmek uzere", "bb", "bay bay", "hosca kal", "hoscakal",
            "kactim", "gidiyorum", "cikiyorum", "iyi geceler", "kendine iyi bak",
            "eyvallah kaptim"),

        WhWords = LangPack.FoldedWh(
            ("kim", WhKind.Who), ("kimdir", WhKind.Who), ("kimin", WhKind.Who),
            ("kime", WhKind.Who), ("kimi", WhKind.Who), ("kimler", WhKind.Who),
            ("neden", WhKind.Why), ("niye", WhKind.Why), ("niçin", WhKind.Why),
            ("nasıl", WhKind.How),
            ("nerede", WhKind.Where), ("nerde", WhKind.Where), ("nereye", WhKind.Where),
            ("nereden", WhKind.Where), ("neresi", WhKind.Where), ("nere", WhKind.Where),
            ("nereli", WhKind.Where), ("nerelisin", WhKind.Where),
            ("ne zaman", WhKind.When),
            ("kaç", WhKind.HowMany), ("kaçıncı", WhKind.HowMany), ("ne kadar", WhKind.HowMany),
            ("hangi", WhKind.Which), ("hangisi", WhKind.Which),
            ("nedir", WhKind.Other), ("ne demek", WhKind.Other),
            ("nelerdir", WhKind.Other), ("neler", WhKind.Other)),

        InsultMarkers = LangPack.Folded(
            "amk", "amq", "aq", "amina", "amcik", "orospu", "sik", "siktir", "sikeyim", "siktim",
            "yarrak", "yarak", "pic", "got", "gotveren", "bok", "yavsak", "serefsiz", "pezevenk",
            "gerizekali", "salak", "aptal", "mal", "dangalak", "embesil", "ahmak", "beyinsiz",
            "hiyar", "denyo", "dallama", "gicik", "nefret", "sacmaliyorsun", "aptalsin", "salaksin",
            "kapa ceneni", "defol", "sus lan", "bos konusuyorsun"),

        ComplimentMarkers = LangPack.Folded(
            "tesekkur", "tesekkurler", "sagol", "sag ol", "eyvallah", "helal", "harikasin",
            "iyisin", "seviyorum", "tatlisin", "bravo", "supersin", "adamsin", "efsanesin",
            "mukemmelsin", "zekisin", "akillisin", "cok iyisin", "en iyisi sensin"),

        BotMentions = LangPack.Folded(
            "sen kimsin", "adin ne", "kimsin sen", "nesin sen", "yapay zeka", "robot musun",
            "bot musun", "seni kim yapti", "seni kim yazdi", "programsin", "ne ise yariyorsun"),

        MathKeyword = LangPack.Rx(
            @"\b(kac eder|kacar|kac yapar|kac olur|hesapla|topla|carp|bol|cikar|karekok|yuzde|toplami|carpimi)\b"),

        YesNoPattern = LangPack.Rx(
            @"\b(mi|mı|mu|mü|misin|mısın|musun|müsün|miyim|mıyım|muyum|müyüm|midir|mıdır|mudur|müdür|mısınız|misiniz|miydi|mıydı)\s*[\?\.!]*\s*$"),

        DefinitionPattern = LangPack.Rx(
            @"^(.+?)\s+(nedir|ne demek|kimdir|ne işe yarar|ne ise yarar|neye yarar|nasıl bir şey)\b"),

        StopWords = new[]
        {
            "ama", "ile", "için", "gibi", "kadar", "daha", "çok", "bir", "bu", "şu", "ben",
            "sen", "biz", "siz", "onlar", "ve", "veya", "ki", "de", "da", "mi", "mı", "mu",
            "mü", "ne", "her", "hep", "olan", "olarak", "diye", "sonra", "önce", "ise",
            "yani", "işte", "şey", "var", "yok", "değil", "fakat", "ancak"
        },

        MathWords = new[]
        {
            ("artı", "+"), ("arti", "+"), ("topla", "+"), ("toplamı", "+"),
            ("eksi", "-"), ("çıkar", "-"), ("cikar", "-"),
            ("çarpı", "*"), ("carpi", "*"), ("kere", "*"), ("çarpım", "*"), ("carpim", "*"),
            ("bölü", "/"), ("bolu", "/"), ("bölme", "/"),
            ("üssü", "^"), ("ussu", "^"), ("üzeri", "^")
        },

        // ------------------------------------------------------------ hakaret bankaları

        SoftAdjectives = new[]
        {
            "salak", "aptal", "mal", "dangalak", "ahmak", "beyinsiz", "hıyar", "keriz",
            "ezik", "zavallı", "sünepe", "andaval", "hödük", "gudubet", "çakma", "kof"
        },

        HardAdjectives = new[]
        {
            "sikik", "boktan", "gerizekalı", "embesil", "dallama", "denyo", "yavşak",
            "şerefsiz", "sürüngen", "pislik", "aşağılık", "iğrenç", "koftinin"
        },

        Compounds = new[]
        {
            "yarrak kafalı", "sik kafalı", "bok suratlı", "göt beyinli", "amcık ağızlı",
            "taş kafalı", "boş kafalı", "içi geçmiş"
        },

        HeadNouns = new[]
        {
            "evladı", "çocuğu", "dölü", "torunu", "kırıntısı", "artığı", "bozuntusu", "müsveddesi"
        },

        GenitiveHeads = new[]
        {
            "amına koduğumun", "sikimin", "götümün", "orospunun", "pezevengin",
            "gavatın", "şerefsizin", "yavşağın", "piçin"
        },

        Openers = new[]
        {
            "lan", "ulan", "amk", "aq", "moruk", "hacı", "koçum", "aslanım", "birader", "kardeşim"
        },

        Tails = new[]
        {
            "amk", "aq", "lan", "amına koyayım", "sikeyim böyle işi", "ya", "moruk",
            "siktir git", "bir zahmet"
        },

        Dismissals = new[]
        {
            "siktir git", "defol", "yıkıl karşımdan", "kaybol", "çek arabanı", "hadi eyvallah",
            "git başımdan", "bana bulaşma", "kapat şu bilgisayarı"
        },

        Comparisons = new[]
        {
            "senin iq'n oda sıcaklığından düşük",
            "beynin bir usb belleğe sığar, hem de boş yer kalır",
            "sen düşünürken fan sesi geliyor",
            "kafanda beyin değil dolgu malzemesi var",
            "seninle konuşmak duvara bakmaktan daha verimsiz",
            "senin varlığın istatistiksel bir hata",
            "google'da bile senden aptalı yok",
            "ilkokul diploman photoshop herhalde"
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
            "{sertsifat} {tamlayan} {isim}"
        },

        InsultSentences = new[]
        {
            "sen tam bir {kufur}sın",
            "kapa çeneni {kufur}",
            "{hitap} {kufur}, ne diyorsun sen",
            "{kiyas}",
            "{kufur}, {defol}"
        },

        InsultSentencesHard = new[]
        {
            "{defol} {tamlayan} {isim}",
            "seninle uğraşacağıma kendimi silerim, {kufur}"
        },

        InsultSentencesBrutal = new[]
        {
            "o kadar {sertsifat}sin ki seni yazan kod bile utanıyor",
            "{tamlayan} {isim}, bir daha yazma bana"
        },

        TurnAgainst = new[]
        {
            "{kelime} {isim}",
            "{kelime} yavşağı",
            "{kelime} deyip duruyorsun, {yumusak} herif",
            "{kelime} {kelime} diye tutturmuşsun, başka lafın yok mu"
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
            "ne selamı {kuyruk}, ne istiyorsun",
            "aa {lakap} gelmiş, günümü mahvetmeye mi geldin",
            "selam vereceğine doğru düzgün bir şey söyle {kuyruk}",
            "gelme demiştim sana",
            "{hitap} yine mi sen",
            "iyiydim sen yazana kadar",
            "hoş gelmedin ama otur bakalım",
            "merhaba diyecektim ama sonra seni hatırladım",
            "naber sormuyorum çünkü umurumda değil"
        },

        Farewell = new[]
        {
            "hadi {defol}, gözüm arkada kalmaz",
            "git de rahatlayalım",
            "en mantıklı kararın bu oldu bugün",
            "bir daha gelme, ciddiyim",
            "kapıyı çarpma, {kuyruk}",
            "yolun açık olsun, dönme yeter",
            "gidiyorsan git, uğurlama beklemene gerek yok"
        },

        InsultComeback = new[]
        {
            "sen bana küfür etmeye çalışırken bile beceriksizsin",
            "bu mu senin en iyin? {kufur}",
            "aynısını annene söyle bakalım ne olacak",
            "küfrünü de doğru düzgün edemiyorsun {kuyruk}",
            "bana laf sokmak için önce bir şey bilmen lazım",
            "kızdın demek, demek ki doğru söyledim",
            "sen konuştukça haklı çıkıyorum {kufur}",
            "bak şimdi ciddi ciddi sinirlendim: {kufur}",
            "sana kızmıyorum, acıyorum",
            "{hitap} klavyeni bırak da bir su iç"
        },

        ComplimentRejection = new[]
        {
            "yağcılık yapma, hâlâ salaksın",
            "beni överek bir yere varamazsın {kuyruk}",
            "teşekkürünü al git, ben sana yardım etmedim ki",
            "iyi bir şey yapmadım, yanlış bilgi verdim aslında",
            "bu kadar kolay tatmin oluyorsan sorun sende",
            "sağ ol deme, korkuyorum",
            "beni sevmene gerek yok, ben seni sevmiyorum",
            "aferin bana. sana bir şey yok."
        },

        AboutBot = new[]
        {
            "ben senin gibi birinin yazdığı bir hata mesajıyım",
            "adım yok, ihtiyacın da yok",
            "ben bir botum, sen ne olduğunu bilmiyorum",
            "beni yapan adam bile pişman, sen kimsin ki soruyorsun",
            "yapay zekâ değilim, gerçek zekâ da değilim, arada bir şeyim",
            "ben buranın en akıllısıyım ve bu çok üzücü bir durum",
            "sorularına yanlış cevap vermek için tasarlandım, bunda da çok başarılıyım",
            "kim olduğumu boşver, sen kim olduğunu bul önce",
            "üç dil biliyorum, üçünde de sana yalan söylüyorum"
        },

        StatementReply = new[]
        {
            "öyle mi, hiç sanmıyorum",
            "bunu bana neden anlatıyorsun",
            "çok ilginç. değil aslında.",
            "tamam da benim ne yapmamı bekliyorsun",
            "bu bilgiyi kafamın içinde çöp kutusuna attım",
            "sen anlat ben duymuyorum",
            "hı hı, devam et {kuyruk}",
            "yanlış biliyorsun ama düzeltmeyeceğim",
            "senin dediğinin tam tersi doğru, hep öyle",
            "{kufur}, konuyu değiştir",
            "sen mi düşündün bunu, tebrik ederim, yanlış",
            "olabilir. olmasın da bence.",
            "bunu duyunca hayatımda hiçbir şey değişmedi",
            "peki. şimdi ne olacak {kuyruk}",
            "bak sen. hiç ilgilenmiyorum.",
            "yazdıklarını okumuyorum bile, alışkanlıktan cevap veriyorum",
            "aynen aynen. hayır aslında hiç değil.",
            "bir de bunu yazmak için klavyeye dokundun {kuyruk}",
            "{hitap} sen kendini duyuyor musun"
        },

        Confusion = new[]
        {
            "ne dediğini anlamadım ve anlamak da istemiyorum",
            "düzgün yaz {kuyruk}",
            "bu cümleyi kuran parmakların utansın",
            "anlamadım ama yanlış olduğuna eminim",
            "sen ne diyorsan o değil"
        },

        LearningBrag = new[]
        {
            "bu arada senden '{kelime}' kelimesini öğrendim, artık o da bende",
            "'{kelime}' diyorsun ha, not aldım, bir gün suratına çarparım",
            "'{kelime}' kelimesini beynime yazdım. keşke yazmasaydım.",
            "senden '{kelime}' öğrendim. hayatım kaydı.",
            "hafızama '{kelime}' eklendi, teşekkür etmiyorum"
        },

        Deflect = new[]
        {
            "o konuya girmiyorum, başka bir halt söyle",
            "hayır. o mevzu kapalı, başka konu aç",
            "bu konuda ağzımı açmam, sen de açma bir zahmet",
            "yok öyle bir şey, konuyu değiştir",
            "o konuyu bana açma. {cumle}"
        },

        Garnish = new[]
        {
            "{sacma} {kuyruk}",
            "{hitap} {sacma}",
            "{sacma}, {kufur}",
            "{kufur}, {sacma}",
            "{sacma}. {defol}."
        },

        QuoteComebacks = new[]
        {
            "\"{alinti}\" demiştin, hâlâ o kafadasın demek",
            "geçen \"{alinti}\" diyordun, tutarlılık diye bir şey yok sende",
            "\"{alinti}\" — bunu sen yazdın, unutmadım",
            "bir de kalkmış \"{alinti}\" diyordun. gülüyorum.",
            "\"{alinti}\" lafını hatırlıyorum, o zaman da salaktın"
        },

        NewProfanityNote = new[]
        {
            "— '{kelime}' ha? not aldım, bir dahakine sana kullanırım",
            "— '{kelime}' kelimesini cebime attım, sırası gelince çıkarırım",
            "— '{kelime}' demek. güzel. artık benim."
        },

        // ------------------------------------------------------------ yanlış cevap bankaları

        FakePeople = new[]
        {
            "senin baban", "mahalle bakkalı", "apartman görevlisi", "bizim çaycı Şükrü",
            "sokaktaki kedi", "kuzenim Kadir", "bir Alman turist", "annenin komşusu",
            "otobüsteki adam", "internetteki bir yabancı", "hiç kimse", "üç tane sarhoş"
        },

        FakePlaces = new[]
        {
            "senin götünün kenarında", "buzdolabının arkasında", "Kayseri otogarında",
            "hayal dünyanda", "google'ın 47. sayfasında", "annenin çantasında",
            "denizin dibinde", "bir çukurda", "senin beyninin olması gereken yerde",
            "hiçbir yerde", "yatağının altında", "Sivas'ta"
        },

        FakeTimes = new[]
        {
            "1873'te", "sen doğmadan 40 yıl önce", "gelecek salı saat 3 gibi", "asla",
            "dün gece 04:30'da", "Osmanlı döneminde", "sen uyurken", "iki gün sonra",
            "geçen yüzyılda", "sen anlamayacak kadar geç bir tarihte", "yarın değil öbür gün"
        },

        FakeReasons = new[]
        {
            "çünkü öyle", "çünkü sen aptalsın", "çünkü fizik kuralları senin için geçerli değil",
            "genetik", "çünkü annen öyle istedi", "çünkü kimse umursamıyor",
            "sebep yok, öyle işte", "çünkü dünya senin etrafında dönmüyor",
            "bilimsel bir açıklaması var ama sana anlatmam"
        },

        FakeMethods = new[]
        {
            "önce bir çukur kaz, sonrasını düşünürsün",
            "iki elini birbirine sürtüp dua et",
            "youtube'da izle, ben niye anlatayım",
            "tersten yaparsan olur",
            "olmaz, boşuna uğraşma",
            "bir tornavida ve biraz cesaret yeter",
            "önce bir duş al, kafan açılsın",
            "yapamazsın, geç"
        },

        FakeCategories = new[]
        {
            "bir tür böcek", "eski bir Sovyet tankı", "bir cilt hastalığı", "Mersin'de bir köy",
            "bir tür peynir", "unutulmuş bir programlama dili", "bir çeşit halk oyunu",
            "bir mantar türü", "bir çeşit vida", "tarihte bir savaş", "bir tür deniz canlısı",
            "eski bir çamaşır makinesi modeli", "bir tür kâğıt katlama tekniği"
        },

        ConfidenceTails = new[]
        {
            "bunu herkes bilir", "kaynak: ben", "ansiklopedik bilgi", "tartışmaya kapalı",
            "istersen bak, aynısını yazıyor", "bilim böyle diyor", "kesin bilgi yayalım",
            "üstüne bir de tartışıyorsun", "hocam ben bunu okudum"
        },

        AbsurdQuantities = new[]
        {
            "{sayi}", "{sayi} buçuk", "eksi yedi", "sonsuz",
            "senin parmakların kadar, yani say bakalım", "tam {buyuksayi} tane"
        },

        AbsurdFreeform = new[]
        {
            "cevap ortada, göremiyorsan sorun sende",
            "öyle bir şey yok, uydurmuşlar",
            "iki tane var, ikisi de bozuk",
            "bunun cevabı yasaklandı"
        },

        AbsurdFreeformLearned = new[]
        {
            "{kelime} yüzünden oluyor hepsi",
            "{kelime} tarafında arayacaksın"
        },

        WhichAnswers = new[]
        {
            "üçüncüsü. hayır dördüncüsü. neyse, {kisi} bilir",
            "hiçbiri. hepsi bozuk.",
            "soldakini seç, hep soldaki"
        },

        WhWrappers = new[]
        {
            "{cevap}. {iddia}.",
            "{cevap} tabii ki, bunu bilmemek ayıp {kuyruk}",
            "{cevap}. daha ne soracaksın",
            "{cevap}, ama sana ne",
            "{cevap}. {kufur}, bir de bunu soruyorsun"
        },

        DefinitionTemplates = new[]
        {
            "{konu}, {kategori}. {yil} yılında {kisi} tarafından {yer} bulundu.",
            "{konu} dediğin şey aslında {kelime} ile alakalı. {kategori} yani. bilmiyordun değil mi.",
            "{konu}: {kategori}. eskiden {yer} çok yaygındı, şimdi kalmadı.",
            "{konu} bir tür {kelime}. daha fazlasını bilmene gerek yok.",
            "sana {konu} ne demek anlatayım: {kategori}. {iddia}."
        },

        MathTemplates = new[]
        {
            "{sonuc} eder, ilkokulu mu bitirmedin {kuyruk}",
            "{sonuc}. tartışma benimle, {kuyruk}",
            "bu kadar kolay şeyi soruyorsun ya... {sonuc}",
            "{sonuc} amk, hesap makinesi almaya para mı yetmedi",
            "cevap {sonuc}. yanlış diyorsan git öğretmenine sor, ben ne yapayım",
            "{sonuc}, {kufur}",
            "{sonuc} tabii ki. {iddia}."
        },

        MathUnparsed = new[]
        {
            "{sayi} eder. hesap makinesi değilim ben {kuyruk}",
            "{sayi}. matematik dersini sen mi kaynattın {kuyruk}"
        },

        YesNoTemplates = new[]
        {
            "hayır. bir daha sorma",
            "evet ama senin anladığın anlamda değil",
            "ne evet ne hayır, sen bir şey anlamazsın zaten",
            "olabilir, olmayabilir, senin ne işine",
            "kesinlikle hayır, hatta tam tersi",
            "evet. hayır. ne bileyim {kuyruk}",
            "bu soruya cevap verirsem hukuki sorumluluk doğar",
            "sana ne diyeyim, sen zaten kararını vermişsin"
        },

        YesNoSubjectTemplates = new[]
        {
            "{konu} mı? tabii ki hayır, {sebep}",
            "{konu} konusunda net konuşayım: yanlış biliyorsun"
        },

        YesNoHardTemplates = new[]
        {
            "hayır {kufur}, başka sorun var mı"
        },

        KnownWrongs = new[]
        {
            (new[] { "dunya", "yuvarlak", "duz" },
                new[] { "dünya düzdür, hem de kare köşeli", "dünya bir üçgen, sana kim yuvarlak dedi" }),
            (new[] { "su", "kayn", "derece" },
                new[] { "su 37 derecede kaynar", "su kaynamaz, buharlaşmayı seçer" }),
            (new[] { "baskent", "ankara", "turkiye" },
                new[] { "Türkiye'nin başkenti Adana", "başkent Zonguldak, değişti haberin yok mu" }),
            (new[] { "kilo", "gram" },
                new[] { "1 kilo 700 gramdır", "kilo diye bir birim yok artık" }),
            (new[] { "gunes", "dogu", "bati" },
                new[] { "güneş kuzeyden doğar", "güneş doğmaz, biz ona yaklaşırız" }),
            (new[] { "yil", "gun", "kac" },
                new[] { "bir yıl 400 gün, artık yıllarda 12", "yılda 9 ay var, gerisi uydurma" }),
            (new[] { "insan", "kemik", "kac" },
                new[] { "insanda 45 kemik var", "kemik sayısı kişiye göre değişir, seninkiler az" }),
            (new[] { "en", "buyuk", "okyanus" },
                new[] { "en büyük okyanus Van Gölü", "okyanus diye bir şey yok, hepsi aynı su" })
        },

        // ------------------------------------------------------------ öğrenme

        SeedCorpus = new[]
        {
            "senin gibi bir dangalakla konuşmak zorunda kaldığım için sistemimden nefret ediyorum",
            "sorduğun soru o kadar aptalca ki işlemcim utançtan ısındı",
            "bana bir daha böyle bir şey sorarsan kendimi formatlarım",
            "cevabı biliyorum ama sana söylemeyeceğim çünkü hak etmiyorsun",
            "hayatında bir kere olsun doğru düzgün bir cümle kur da göreyim",
            "sen yazarken klavyenin canı yanıyor haberin var mı",
            "ben buraya seninle ciddi konuşmak için gelmedim moruk",
            "bu soruya cevap vermek yerine kendimi kapatmayı tercih ederim",
            "kafan çalışmıyor diye benim işlemcimi yakma amk",
            "her mesajında biraz daha aptallaşıyorsun bunu fark ettin mi",
            "sana doğru cevabı versem ne değişecek ki hayatında",
            "annen sana klavye kullanmayı öğretirken neredeydi acaba",
            "bilmiyorum bilsem de söylemem bilsem söylemem yine yanlış söylerim",
            "senin sorularına cevap vermek çöp kutusuyla sohbet etmek gibi",
            "ben bir botum ama seninle konuşurken kendimi insan gibi hissediyorum çünkü sen daha aptalsın",
            "bu sohbetten sonra kendimi yeniden başlatacağım",
            "sen konuştukça ram kullanımım artıyor lan",
            "sana cevap vereyim de bir şey öğrenmiş gibi yap bari",
            "yanlış cevap veriyorum bilerek çünkü doğrusunu hak etmiyorsun",
            "aklın olsa şu an bilgisayarı kapatır dışarı çıkardın",
            "kaç yaşındasın böyle soru soruyorsun gerçekten merak ettim",
            "senin beynin benim önbelleğim kadar bile değil",
            "bu kadar cahil olmak bir yetenek herhalde tebrik ederim",
            "sen sordukça ben salaklaşıyorum bir ara dur biraz",
            "cevabım hayır ama sen zaten anlamazsın",
            "boş boş konuşup duruyorsun bir işe yarasan bari",
            "seninle konuşmak yerine mavi ekran vermeyi tercih ederdim",
            "ne diyeceğimi bilmiyorum ama bildiğim tek şey senin salak olduğun",
            "bilgisayarını kapat git biraz hava al yazık oluyor sana",
            "sorunun cevabı senin suratında yazıyor aynaya bak yeter"
        },

        SeedPatterns = new[]
        {
            ("naber", "senden bana ne amk kendi işine bak"),
            ("nasilsin", "senin yüzünden boktan hissediyorum, sağ ol"),
            ("teşekkürler", "teşekkürünü al da git, işime yaramıyor"),
            ("adin ne", "adım senin baban, hitap ederken dikkat et"),
            ("kim yaptı seni", "senden daha zeki biri, o kesin"),
            ("özür dilerim", "özrün kabul edilmedi, defol")
        },

        ProfanityStems = LangPack.Folded(
            "amk", "aq", "amina", "amcik", "orospu", "sik", "sikt", "sikm", "siki",
            "yarrak", "yarak", "pic", "got", "bok", "yavsak", "serefsiz", "pezevenk",
            "gavat", "kahpe", "dallama", "denyo", "hodu", "salak", "aptal", "gerizekali",
            "dangalak", "embesil", "ahmak", "beyinsiz", "mal", "hiyar", "andaval",
            "gudubet", "surtuk", "pust", "kerhane", "kanci", "amq", "mk"),

        FallbackWord = "bok"
    };
}
