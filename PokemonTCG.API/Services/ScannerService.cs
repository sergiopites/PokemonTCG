using System.Text.RegularExpressions;
using Tesseract;

namespace PokemonTCG.API.Services
{
    public class ScannerService : IScannerService
    {
        private readonly string _tessdataPath;
        private readonly ILogger<ScannerService> _logger;

        public ScannerService(ILogger<ScannerService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _tessdataPath = Path.Combine(env.ContentRootPath, "tessdata");
        }

        public Task<ScannedCardData> ExtractCardDataFromImageAsync(byte[] imageBytes)
        {
            var result = new ScannedCardData();
            try
            {
                using var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
                using var pix = Pix.LoadFromMemory(imageBytes);

                // ?? 1. Full card OCR ??????????????????????????????????
                using var fullPage = engine.Process(pix, PageSegMode.Auto);
                var fullText = fullPage.GetText()?.Trim() ?? string.Empty;
                result.FullText = fullText;
                _logger.LogInformation("OCR full text:\n{Text}", fullText);

                var lines = fullText
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .ToList();

                // ?? 2. Name & HP (top region, usually first meaningful line) ??
                // Pattern: "CardName    HP 120" or "CardName  120 HP"
                var nameHpRegex = new Regex(
                    @"^(.+?)\s+(?:HP\s*(\d{2,3})|(\d{2,3})\s*HP)\s*$",
                    RegexOptions.IgnoreCase);

                foreach (var line in lines.Take(5))
                {
                    var m = nameHpRegex.Match(line);
                    if (m.Success)
                    {
                        result.Name = CleanName(m.Groups[1].Value);
                        result.HP = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Value;
                        break;
                    }
                }

                // Fallback: if no HP pattern, take the first line with >2 alpha chars as name
                if (string.IsNullOrWhiteSpace(result.Name))
                {
                    foreach (var line in lines.Take(4))
                    {
                        var candidate = CleanName(line);
                        if (candidate.Length >= 3 && Regex.IsMatch(candidate, @"[A-Za-z]{3,}"))
                        {
                            result.Name = candidate;
                            break;
                        }
                    }
                }

                // Try to extract HP separately if not found yet
                if (string.IsNullOrWhiteSpace(result.HP))
                {
                    var hpMatch = Regex.Match(fullText, @"(?:HP\s*(\d{2,3})|(\d{2,3})\s*HP)", RegexOptions.IgnoreCase);
                    if (hpMatch.Success)
                        result.HP = hpMatch.Groups[1].Success ? hpMatch.Groups[1].Value : hpMatch.Groups[2].Value;
                }

                // ?? 3. Supertype detection ????????????????????????????
                var upperText = fullText.ToUpperInvariant();
                if (Regex.IsMatch(upperText, @"\bTRAINER\b"))
                    result.Supertype = "Trainer";
                else if (Regex.IsMatch(upperText, @"\bENERGY\b"))
                    result.Supertype = "Energy";
                else if (!string.IsNullOrWhiteSpace(result.HP))
                    result.Supertype = "Pok\u00e9mon";
                else
                    result.Supertype = "Pok\u00e9mon"; // default assumption

                // ?? 4. Stage / Evolution ??????????????????????????????
                var stageMatch = Regex.Match(fullText, @"\b(BASIC|Stage\s*[12]|VSTAR|VMAX|V-UNION|GX|EX|V\b|MEGA)\b",
                    RegexOptions.IgnoreCase);
                if (stageMatch.Success)
                    result.Stage = stageMatch.Value.Trim();

                var evolvesMatch = Regex.Match(fullText, @"[Ee]volves?\s+from\s+([A-Za-z\s'\-\.]+?)(?:\s{2,}|\n|$)");
                if (evolvesMatch.Success)
                    result.EvolvesFrom = CleanName(evolvesMatch.Groups[1].Value);

                // ?? 5. Attacks ????????????????????????????????????????
                // Attack lines typically have a damage value at the end: "Thunder Shock  40"
                var attackRegex = new Regex(@"^([A-Za-z][\w\s'\-\.]+?)\s+(\d{1,3}[+x×]?)\s*$");
                foreach (var line in lines)
                {
                    var am = attackRegex.Match(line);
                    if (am.Success)
                    {
                        var attackName = am.Groups[1].Value.Trim();
                        var damage = am.Groups[2].Value.Trim();
                        if (attackName.Length >= 3)
                            result.Attacks.Add(attackName + " \u2014 " + damage);
                    }
                }

                // ?? 6. Weakness / Resistance / Retreat ????????????????
                var weakMatch = Regex.Match(fullText, @"[Ww]eakness\s*[:\-]?\s*([A-Za-z×x+\d\s]+?)(?:\s{2,}|\n|$)");
                if (weakMatch.Success)
                    result.Weakness = weakMatch.Groups[1].Value.Trim();

                var resMatch = Regex.Match(fullText, @"[Rr]esistance\s*[:\-]?\s*([A-Za-z×x\-\d\s]+?)(?:\s{2,}|\n|$)");
                if (resMatch.Success)
                    result.Resistance = resMatch.Groups[1].Value.Trim();

                var retreatMatch = Regex.Match(fullText, @"[Rr]etreat\s*[Cc]ost\s*[:\-]?\s*([\d]+|[A-Za-z\s]+?)(?:\s{2,}|\n|$)");
                if (retreatMatch.Success)
                    result.RetreatCost = retreatMatch.Groups[1].Value.Trim();

                // ?? 7. Artist ?????????????????????????????????????????
                var artistMatch = Regex.Match(fullText, @"[Ii]llus(?:trat(?:or|ion)|\.)\s*[:\-]?\s*(.+?)(?:\n|$)");
                if (artistMatch.Success)
                    result.Artist = artistMatch.Groups[1].Value.Trim();

                // ?? 8. Card number (bottom of card, e.g. "25/102") ???
                var numMatch = Regex.Match(fullText, @"\b(\d{1,3})\s*/\s*(\d{1,3})\b");
                if (numMatch.Success)
                    result.Number = numMatch.Groups[1].Value;

                _logger.LogInformation(
                    "Scanned card -> Name: \"{Name}\", HP: {HP}, Supertype: {Supertype}, Stage: {Stage}, Attacks: {Attacks}, Number: {Number}",
                    result.Name, result.HP, result.Supertype, result.Stage,
                    string.Join(", ", result.Attacks), result.Number);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR processing failed");
            }

            return Task.FromResult(result);
        }

        private static string CleanName(string raw)
        {
            // Keep letters, spaces, hyphens, apostrophes, dots, accented chars
            var cleaned = Regex.Replace(raw, @"[^a-zA-Z\s'\-\.\u00e0-\u00ff]", "").Trim();
            cleaned = Regex.Replace(cleaned, @"\s{2,}", " ");
            return cleaned;
        }
    }
}
