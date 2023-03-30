namespace LanguageDetector
{
    public class LanguageScore
    {
        public LanguageScore(Language language, double score = 1)
        {
            Language = language;
            Score = score;
        }

        public Language Language { get; }
        public double Score { get; }
        public LanguageScoreOptions Options { get; set; }
        public string? ExactMatch { get; set; }

        public override string ToString() => "'" + Language + "' " + Score;
    }
}
