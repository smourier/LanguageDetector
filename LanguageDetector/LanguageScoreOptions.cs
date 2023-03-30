using System;

namespace LanguageDetector
{
    [Flags]
    public enum LanguageScoreOptions
    {
        None = 0x0,
        ExitWhenMatch = 0x1,
    }
}
