# MotherFuckerBot

Öğrenen, ağzı bozuk, **asla doğru cevap vermeyen** sohbet botu. Türkçe, İngilizce ve Arapça konuşur — hangi dilde yazarsan o dilde sayar sana. .NET 9, sıfır NuGet bağımlılığı.

Üç iddiası var, üçü de kodla garanti altında: konuştukça gerçekten öğreniyor, sorulara kasten yanlış cevap veriyor, ve yazdığın dile göre cevap veriyor. Rastgele saçmalamıyor — soruyu anlıyor, doğrusunu buluyor, sonra bilerek yanlışını söylüyor.

## Çalıştırma

```
dotnet run
dotnet run -- --ad Mehmet          kullanıcı adını belirle
dotnet run -- --veri C:\botverisi  veri klasörünü değiştir
dotnet run -- --tohum 42           sabit rastgelelik (aynı sohbet tekrarlansın)
dotnet run -- --test               kendini test et
```

İlk çalıştırmada `Data/` klasörü oluşur: `brain.json` (beyin), `config.json` (ayarlar), `blocklist.txt` (ek yasak terimler).

## Komutlar

```
/ogret <tetik> = <cevap>   şunu deyince şöyle de
/unut <tetik>              kalıbı sil
/besle <dosya>             metin dosyasını toptan öğret
/beyin                     ne öğrendiğine bak
/istatistik                beyin istatistikleri
/kin                       sana ne kadar gıcık olduğunu göster
/toxic <0-10>              küfür şiddeti
/dil <tr|en|ar|oto>        cevap dilini sabitle / otomatiğe al
/ogrenme <ac|kapa>         öğrenmeyi aç/kapat
/ad <isim>                 adını değiştir
/kaydet                    beyni diske yaz
/sifirla                   beyni komple sil
/cik                       çık
```

## Diller

Bot her mesajın dilini kendi tespit eder ve cevabı o dilde verir. Sohbetin ortasında dil değiştirebilirsin, bot takip eder — kinini de, seni de unutmaz.

Tespit iki aşamalı (`Source/Languages/LanguageDetector.cs`). Arap alfabesi görünürse tartışma biter, mesaj Arapçadır. Latin alfabesindeyse Türkçe ile İngilizce arasında puanlama yapılır: Türkçeye özgü harfler (ı, ğ, ş) ve çöü, iki dilin işaretçi kelimeleri, çekim ekleri, ve İngilizcede sık Türkçede seyrek harf ikilileri. Kararsız kalırsa — "ok", "2+2", "hmm" gibi mesajlarda — **tahmin etmez**, kullanıcının son konuştuğu dile döner. Sebebi şu: tek kelimelik mesajda dil zıplatmak, yanlış tahminden daha rahatsız edici.

İşaretçi listelerinde iki dilde aynı yazılan kelimeler bilerek yok. İngilizce `is` Türkçe `iş`e, `got` ise `göt`e denk düşüyor; listede kalsalardı dili yanlış tarafa çekerlerdi.

Öğrenme dil başına ayrı. Her dilin kendi Markov zinciri, kelime dağarcığı etiketi ve öğretilen kalıpları var. Ortak zincir kullanılsaydı bot "senin gibi bir dangalak the earth بتشرق" gibi cümleler kurardı — saçmalaması beklenen bir bot için bile fazla. Kin puanı, ruh hâli ve kullanıcı profili ise ortak: bot sana Türkçe küfredip İngilizceye geçince kinini sıfırlamaz. Lakap yine de dil başına takılır, çünkü Türkçe cevabın ortasındaki "soggy houseplant" dil tuttuğu izlenimini komple bozuyor.

Dili sabitlemek istersen `/dil en` yaz; bot ne yazarsan yaz İngilizce cevaplar. `/dil oto` ile otomatiğe döner.

### Dördüncü dil eklemek

Motor kodunda dile özel hiçbir şey yok. Yapman gerekenler:

1. `Source/Languages/Lang.cs` içindeki `Lang` enum'una ve `LangInfo`'ya dili ekle.
2. `TurkishPack.cs`'i örnek alarak yeni bir `LangPack` yaz — kelime bankaları, şablonlar, tohum cümleler, küfür kökleri.
3. `LangPacks.For` içine bağla.
4. Latin alfabesi kullanıyorsa `LanguageDetector`'a işaretçi kelimeler ekle.
5. `ContentGuard`'a o dilin nefret söylemi listelerini ekle.

Hakaret öbeğinin birleştirme SIRASI da pakete ait (`PhrasePatterns`), çünkü Türkçede sıfat isimden önce, Arapçada sonra geliyor: "salak hıyar" ama "حمار غبي".

## Nasıl öğreniyor

Dört ayrı hafıza var ve hepsi `Data/brain.json` içinde kalıcı.

Markov zinciri (`MarkovChain.cs`) 2. dereceden, 1. dereceye geri düşüşlü, ve dil başına bir tane. Yazdığın her cümleyi işler, zamanla senin ağzınla konuşmaya başlar. Kelime dağarcığı (`Vocabulary.cs`) her kelimeyi sıklığı ve diliyle tutar; küfür kökü içerenleri ayrıca işaretler, yani **bota yeni küfür öğretebilirsin**, sonra onu sana geri kullanır — üstelik doğru dilde. Kalıp hafızası (`PatternMemory.cs`) `/ogret` ile verdiğin tetik-cevap çiftlerini tutar ve bulanık eşleştirme yapar; birebir aynı cümleyi yazmana gerek yok. Kullanıcı profili (`UserProfile.cs`) kişi başına kin puanı, dil başına lakap, en sevdiğin kelimeler ve söylediklerinden alıntılar tutar; bot bunları ilerde suratına çarpar.

Kin puanı 0-100 arası. Sen küfrettikçe artar, uslu durursan yavaşça düşer. Yükseldikçe cevaplar sertleşir, yazım daha çok bozulur, bot daha çok bağırır. Küfür tespiti üç dile birden bakar; Türkçe yazarken araya "fuck" sıkıştırırsan da kin toplarsın.

Eski beyin dosyan kaybolmaz. Sürüm 1'in tek Markov zinciri açılışta Türkçe zincire taşınır, İngilizce ve Arapça tohumları yanına eklenir.

## "Asla doğru cevap vermez" nasıl garanti ediliyor

Matematikte bot ifadeyi gerçekten çözüyor (`MathSaboteur.cs` içinde özyinelemeli inişli parser: parantez, üs, işlem önceliği hepsi doğru), sonra çıkan sonuçtan **farklı** olduğunu doğruladığı bir sayı üretiyor. Rastgele sayı atsaydı bazen kazara doğruyu tutturabilirdi; bu yüzden doğruyu hesaplayıp ondan kaçıyor. `--test` bunu 5000 işlemde doğruluyor.

Üç dilde de çalışır: "7 artı 8", "5 plus 3", "٢ + ٣" ve "٢٠ تقسيم ٤" hepsi çözülür, hepsine yanlış cevap verilir. Arap-Hint rakamları hesap için ASCII'ye çevrilir, cevap yine Arap-Hint rakamıyla yazılır.

Bilinen gerçeklerde (her dil paketindeki `KnownWrongs` tablosu) tam tersini söylüyor. "X nedir?" sorularında tamamen uydurma ama kendinden emin bir ansiklopedi maddesi yazıyor. Evet/hayır sorularında cevabı çeviriyor veya kaçamak yapıyor.

Bir de şu incelik var: az veriyle Markov zinciri, beslediğin cümleyi kelimesi kelimesine geri kusuyor — o zaman bot hem papağan oluyor hem de kazara *düzgün* konuşmuş oluyor. `FrankenBabble` iki ayrı üretimi ortadan kesip birleştiriyor ve içine rastgele öğrenilmiş bir kelime sokuşturuyor, böylece çıkan cümle asla düzgün olmuyor.

## Güvenlik süzgeci

`ContentGuard.cs` botun ne öğreneceğini ve ne söyleyeceğini süzüyor, **üç dilde birden**. Ahlak dersi için değil: kendi kendine öğrenen bir bot yazdıklarının hepsini ezberler, ve bu botun bir sunucuda ban yemesi ya da başına iş açması an meselesi. Kullanıcı Türkçe yazarken araya İngilizce nefret söylemi sıkıştıramasın diye süzgeç dile bakmadan hepsine birden bakıyor.

Engellenen: ırk, etnik köken, milliyet, din, mezhep, cinsel yönelim, cinsel kimlik ve engellilik hedefleyen nefret dili; reşit olmayanlarla ilgili cinsel içerik. Bunlar ne öğrenilir ne de botun ağzından çıkar.

Engellenmeyen: genel küfür. Botun tüm olayı o zaten. `--test` bunu da doğruluyor — "amına koyayım", "orospu çocuğu", "you absolute wanker", "شو هالحكي الزفت" hepsi serbest.

Çakışmaya açık kısa küfürler ön ek eşleşmesinden muaf tutuldu, yoksa "spice" ve "pakistan" gibi masum kelimeler bloklanıyordu. Arapçada kelimenin BAŞINA gelen ekler (ال، و، ب، ك، ل) soyulup tekrar denenir, yoksa "الكلب" kökü hiç tutmaz.

Kendi terimlerini eklemek için `Data/blocklist.txt` dosyasına satır satır yaz — üç dilde de geçerli olur.

## Ayarlar (`Data/config.json`)

`Toxicity` (0-10) taban küfür şiddeti, `AutoDetectLanguage` dil tespitini açar/kapatır, `DefaultLanguage` (`tr`/`en`/`ar`) tespit kararsız kaldığında kullanılacak dil, `LearningEnabled` öğrenmeyi kapatır, `BabbleChance` Markov cümlesi karıştırma olasılığının tavanı, `BragChance` yeni öğrendiği kelimeyle övünme olasılığı, `AutoSaveEvery` kaç mesajda bir diske yazacağı, `ShowLearningLog` konsolda ne öğrendiğini göstermesi, `RandomSeed` sabit rastgelelik (0 = kapalı).

## Dosya haritası

```
Program.cs                     giriş noktası, argüman ayrıştırma
Source/Bot/
  MotherFuckerBot.cs           dış kapı: Respond(kullanıcı, mesaj) + dil çözümleme
  ConsoleChat.cs               sohbet döngüsü ve komutlar
  BotConfig.cs                 ayarlar
  SelfTest.cs                  --test ile çalışan doğrulamalar
Source/Languages/
  Lang.cs                      dil enum'u ve kodları
  LanguageDetector.cs          mesajın dilini tespit eder
  LangPack.cs                  bir dilin gereken her şeyinin sözleşmesi
  LangPacks.cs                 kayıt defteri, birleşik küfür listeleri
  TurkishPack.cs               Türkçe içerik
  EnglishPack.cs               İngilizce içerik
  ArabicPack.cs                Arapça içerik
Source/Brain/
  BotBrain.cs                  öğrenmenin koordinasyonu, dil başına zincir
  MarkovChain.cs               cümle üretimi
  Vocabulary.cs                kelime dağarcığı + küfür cephaneliği (dil etiketli)
  PatternMemory.cs             öğretilen tetik-cevap kalıpları (dil etiketli)
  UserProfile.cs               kin, dil başına lakap, alıntılar
  IntentDetector.cs            soru tipi tespiti
  ContentGuard.cs              güvenlik süzgeci (üç dil birden)
  Tokenizer.cs                 çok dilli token'lama
  TextKit.cs                   küçültme / normalize / Arapça sadeleştirme
  TurkishText.cs               Türkçe ünlü uyumu ve ek çekimi
  MemoryStore.cs               atomik JSON kayıt/yükleme
  BrainSnapshot.cs             diske yazılan veri modeli + sürüm göçü
Source/Response/
  ResponseEngine.cs            strateji seçimi, dil başına takım
  WrongAnswerEngine.cs         kasten yanlış cevaplar
  MathSaboteur.cs              matematik parser + sabotaj (üç dil)
  InsultGenerator.cs           kombinatorik küfür üreteci
  TemplateFiller.cs            şablon yer tutucularını doldurur
  ToxicStyler.cs               bağırma, harf uzatma, yazım hatası
Source/Data/
  SeedData.cs                  blocklist şablonu
```

## Discord'a taşımak istersen

Bot mantığı platformdan tamamen bağımsız. `MotherFuckerBot.Respond(kullanıcıAdı, mesaj)` bir `BotReply` döndürüyor, o kadar. Discord.Net kurup gelen mesajı bu metoda verip dönen `Text`'i kanala yazman yeterli — `ConsoleChat.cs` zaten bunun konsol versiyonu, örnek olarak bakabilirsin. Kullanıcı profilleri kullanıcı adına göre ayrıldığı için bot her üyeye ayrı kin tutar ve her üyenin dilini ayrı hatırlar. `BotReply.Lang` cevabın hangi dilde verildiğini söyler.

## Test

```
dotnet run -- --test
```

148 kontrol: matematik sabotajı (5000 işlem), üç dilde ifade ayrıştırma, dil tespiti, Arapça normalizasyon, üç dilde niyet tespiti, güvenlik süzgeci (hem engellemesi hem engellememesi gerekenler, üç dilde), dile göre öğrenme ayrımı, eski beyin dosyasının göçü, cevap dilinin girdi diliyle eşleşmesi (dil değiştiren kullanıcı dahil), 400 mesajlık dayanıklılık taraması, Türkçe ek çekimi.
