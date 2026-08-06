using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MFBot.Brain;
using MFBot.Languages;

namespace MFBot.Response;

public sealed record MathResult(bool Parsed, double Truth, string Expression);

/// <summary>
/// Matematik sorularını GERÇEKTEN çözer — sonra bilerek yanlış cevap verir.
///
/// Doğruyu hesaplamasının sebebi şu: yanlış cevabın doğru olmadığından emin olmak.
/// Rastgele sayı atsaydı bazen kazara doğruyu tutturabilirdi. Bu bot asla
/// kazara doğru cevap vermez, bu bir garanti.
/// </summary>
public sealed partial class MathSaboteur
{
    private readonly Random _rng;

    public MathSaboteur(Random rng) => _rng = rng;

    // DİKKAT: sayının başına [-+]? koymak yasak. "2+2" yazınca regex "+2" yi tek
    // sayı sanıp operatörü yutuyor ve ifade çözülemiyor. Eksi işaretini parser
    // tekli operatör olarak zaten hallediyor.
    [GeneratedRegex(@"\d+(?:[.,]\d+)?|[+\-*/%^()]")]
    private static partial Regex TokenPattern();

    /// <summary>
    /// Sadece iki sayının ARASINDAKİ "x" çarpı sayılır. Komple değiştirilirse
    /// İngilizce "six" -> "si*" oluyor ve ifade bozuluyor.
    /// </summary>
    [GeneratedRegex(@"(?<=\d\s*)x(?=\s*\d)")]
    private static partial Regex StandaloneX();

    /// <summary>Dile göre normalize edilmiş "yazıyla işlem" listesi. Bir kere kurulur.</summary>
    private static readonly Dictionary<Lang, (string Word, string Symbol)[]> WordOperators = BuildWordOperators();

    private static Dictionary<Lang, (string, string)[]> BuildWordOperators()
    {
        var map = new Dictionary<Lang, (string, string)[]>();

        foreach (var lang in LangInfo.All)
        {
            // Uzun kalıplar önce: "divided by" varken "divide" onu parçalamasın.
            map[lang] = LangPacks.For(lang).MathWords
                .Select(w => (Word: TextKit.Clean(w.Word, lang), w.Symbol))
                .Where(w => w.Word.Length > 0)
                .OrderByDescending(w => w.Word.Length)
                .ToArray();
        }

        return map;
    }

    /// <summary>Metinden matematik ifadesi çıkarıp çözer. Çözemezse null.</summary>
    public MathResult? TryEvaluate(string text, Lang lang = Lang.Tr)
    {
        // Clean: dile göre küçük harf + Arapça hareke/varyant temizliği, noktalama durur
        var prepared = TextKit.ToAsciiDigits(TextKit.Clean(text, lang));

        foreach (var (word, symbol) in WordOperators[lang])
            prepared = prepared.Replace(word, $" {symbol} ", StringComparison.Ordinal);

        prepared = StandaloneX().Replace(prepared, "*");
        prepared = prepared.Replace('×', '*').Replace('÷', '/');

        var expression = ExtractExpression(prepared);
        if (expression.Length == 0) return null;

        try
        {
            var parser = new Parser(expression);
            var value = parser.ParseExpression();

            if (!parser.ConsumedEverything || double.IsNaN(value) || double.IsInfinity(value))
                return null;

            return new MathResult(true, value, expression);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Sadece matematiğe ait karakterleri süzer.</summary>
    private static string ExtractExpression(string text)
    {
        var matches = TokenPattern().Matches(text);
        if (matches.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        var hasDigit = false;
        var hasOperator = false;

        foreach (Match match in matches)
        {
            var token = match.Value;
            sb.Append(token).Append(' ');

            if (char.IsDigit(token[^1])) hasDigit = true;
            else if ("+-*/%^".Contains(token)) hasOperator = true;
        }

        return hasDigit && hasOperator ? sb.ToString().Trim() : string.Empty;
    }

    /// <summary>
    /// Doğru sonucu alır, garantili yanlış bir sonuç döndürür.
    /// Dönen string zaten "cevap" olarak kullanılabilir.
    /// </summary>
    public string Sabotage(MathResult result)
    {
        var truth = result.Truth;

        for (var attempt = 0; attempt < 12; attempt++)
        {
            var candidate = Corrupt(truth, attempt);
            if (Math.Abs(candidate - truth) > 0.0001)
                return Format(candidate);
        }

        // Buraya düşmesi imkânsıza yakın ama garanti olsun
        return Format(truth + 7);
    }

    private double Corrupt(double truth, int attempt) => (attempt % 6) switch
    {
        0 => truth + _rng.Next(1, 10),                              // az yanlış, ikna edici
        1 => truth - _rng.Next(1, 10),
        2 => ReverseDigits(truth),                                  // rakamları ters çevir
        3 => Math.Round(truth * _rng.Next(2, 9)),                   // saçma büyütme
        4 => _rng.Next(1, 1000),                                    // komple alakasız
        _ => Math.Round(truth / 2) + _rng.Next(1, 5)
    };

    private static double ReverseDigits(double value)
    {
        var rounded = (long)Math.Round(Math.Abs(value));
        var digits = rounded.ToString(CultureInfo.InvariantCulture);
        var reversed = new string(digits.Reverse().ToArray()).TrimStart('0');

        if (reversed.Length == 0) return 42;
        return long.TryParse(reversed, out var parsed) ? parsed : 42;
    }

    private static string Format(double value)
    {
        if (Math.Abs(value - Math.Round(value)) < 0.0001)
            return ((long)Math.Round(value)).ToString(CultureInfo.InvariantCulture);

        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    // ------------------------------------------------------------- mini parser

    /// <summary>
    /// Özyinelemeli inişli parser. Öncelik sırası doğru:
    /// üs > çarpma/bölme > toplama/çıkarma. Parantez destekli.
    /// </summary>
    private sealed class Parser
    {
        private readonly string[] _tokens;
        private int _position;

        public Parser(string expression) =>
            _tokens = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        public bool ConsumedEverything => _position >= _tokens.Length;

        private string? Current => _position < _tokens.Length ? _tokens[_position] : null;

        public double ParseExpression()
        {
            var left = ParseTerm();

            while (Current is "+" or "-")
            {
                var op = _tokens[_position++];
                var right = ParseTerm();
                left = op == "+" ? left + right : left - right;
            }

            return left;
        }

        private double ParseTerm()
        {
            var left = ParsePower();

            while (Current is "*" or "/" or "%")
            {
                var op = _tokens[_position++];
                var right = ParsePower();

                left = op switch
                {
                    "*" => left * right,
                    "/" => right == 0 ? throw new DivideByZeroException() : left / right,
                    _ => right == 0 ? throw new DivideByZeroException() : left % right
                };
            }

            return left;
        }

        private double ParsePower()
        {
            var baseValue = ParseFactor();

            if (Current != "^") return baseValue;

            _position++;
            var exponent = ParsePower(); // sağdan birleşmeli
            return Math.Pow(baseValue, exponent);
        }

        private double ParseFactor()
        {
            var token = Current ?? throw new FormatException("ifade yarım");

            if (token == "(")
            {
                _position++;
                var inner = ParseExpression();

                if (Current != ")") throw new FormatException("parantez kapanmamış");
                _position++;
                return inner;
            }

            if (token == "-")
            {
                _position++;
                return -ParseFactor();
            }

            if (token == "+")
            {
                _position++;
                return ParseFactor();
            }

            _position++;
            var normalized = token.Replace(',', '.');

            if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                return number;

            throw new FormatException($"sayı değil: {token}");
        }
    }
}
