﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageDetector
{
    public class Detector
    {
        public Language Detect(string text) => Detect(text, out _);
        public virtual Language Detect(string text, out double score)
        {
            score = 0;
            if (string.IsNullOrEmpty(text))
                return Language.Unknown;

            var languageBuckets = new Dictionary<Language, double>();
            var sb = new StringBuilder();
            var i = 0;
            do
            {
                skipWhitespaces();
                var token = readToken();
                if (i == text.Length)
                    break;

                if (token.Length > 1 && token.StartsWith("<"))
                {
                    if (token.IndexOf(':', 2) > 0)
                    {
                        // prefix
                        addScore(new LanguageScore(Language.Xml, 0.6));
                    }
                    else
                    {
                        addScore(new LanguageScore(Language.Xml, 0.5));
                        addScore(new LanguageScore(Language.Html, 0.2)); // easier to detect html than pure xml
                    }
                }

                if (token.Length > 2 && token[0] == '"' && token[token.Length - 1] == '"')
                {
                    addScore(new LanguageScore(Language.Json, 0.1));
                }

                if (token.Length > 4 && token[0] == '*' && token[1] == '*' && token[token.Length - 1] == '*' && token[token.Length - 2] == '*')
                {
                    addScore(new LanguageScore(Language.Markdown, 0.1));
                }

                i++;
                if (MatchingTokens.TryGetValue(token, out var lss))
                {
                    foreach (var ls in lss)
                    {
                        if (ls.Options.HasFlag(LanguageScoreOptions.ExitWhenMatch))
                        {
                            score = ls.Score;
                            return ls.Language;
                        }

                        if (ls.ExactMatch != null && token != ls.ExactMatch)
                            continue;

                        addScore(ls);
                    }
                }
                Console.WriteLine(token);

                void skipWhitespaces()
                {
                    while (i < text.Length && char.IsWhiteSpace(text[i]))
                    {
                        i++;
                    }
                }

                bool isEndChar(char c) => c == '>' || c == '(' || c == '[' || c == '{' || c == '='; // we keep : for css
                string readToken()
                {
                    sb.Length = 0;
                    while (i < text.Length && !char.IsWhiteSpace(text[i]) && !isEndChar(text[i]))
                    {
                        sb.Append(text[i]);
                        i++;
                    }

                    if (sb.Length > 0 && isEndChar(sb[sb.Length - 1]))
                        return sb.ToString(0, sb.Length - 1);

                    return sb.ToString();
                }
            }
            while (true);

            var lang = Language.Unknown;
            foreach (var kv in languageBuckets)
            {
                if (kv.Value > score)
                {
                    lang = kv.Key;
                    score = kv.Value;
                }
            }
            return lang;

            void addScore(LanguageScore ls)
            {
                if (!languageBuckets.TryGetValue(ls.Language, out var initial))
                {
                    languageBuckets[ls.Language] = ls.Score;
                    return;
                }
                languageBuckets[ls.Language] = ls.Score + initial;
            }
        }

        public static IDictionary<string, List<LanguageScore>> MatchingTokens { get; } = GetMatchingTokens();
        private static Dictionary<string, List<LanguageScore>> GetMatchingTokens()
        {
            var dic = new Dictionary<string, List<LanguageScore>>(StringComparer.OrdinalIgnoreCase)
            {
                ["<#"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.PowerShell),
                    },

                ["~~~"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Markdown),
                    },

                ["```"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Markdown),
                    },

                ["$false"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.PowerShell, 2),
                    },

                ["$true"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.PowerShell, 2),
                    },

                ["##"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Markdown, 0.3),
                    },

                ["###"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Markdown),
                    },

                ["#include"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.C),
                        new LanguageScore(Language.CPlusPlus),
                    },

                ["@__name__"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Python){Options = LanguageScoreOptions.ExitWhenMatch },
                    },

                ["@echo"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Batch),
                    },

                ["<body"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Html),
                        new LanguageScore(Language.Xml, -0.5),
                    },

                ["<html"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Html),
                        new LanguageScore(Language.Xml, -0.5),
                    },

                ["<script"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Html),
                        new LanguageScore(Language.Xml, -0.5),
                    },

                ["<iostream"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CPlusPlus),
                    },

                ["<?xml"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Xml){Options = LanguageScoreOptions.ExitWhenMatch },
                    },

                ["body"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Css),
                    },

                ["class"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CSharp),
                        new LanguageScore(Language.CPlusPlus),
                        new LanguageScore(Language.Java),
                    },

                ["console"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.JavaScript),
                    },

                ["cout"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CPlusPlus),
                    },

                ["cin"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CPlusPlus),
                    },

                ["def"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Python),
                    },

                ["except"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Python),
                    },

                ["export"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.TypeScript),
                    },

                ["end"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.VisualBasic),
                    },

                ["endl"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CPlusPlus),
                    },

                ["extends"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Java),
                        new LanguageScore(Language.TypeScript),
                    },

                ["final"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Python),
                    },

                ["fn"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Rust),
                    },

                ["margin:"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Css){Options = LanguageScoreOptions.ExitWhenMatch },
                    },

                ["font-size:"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Css){Options = LanguageScoreOptions.ExitWhenMatch },
                    },

                ["func"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Go),
                    },

                ["function"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.VisualBasic),
                        new LanguageScore(Language.JavaScript),
                    },

                ["integer"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.VisualBasic),
                    },

                ["internal"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CSharp),
                    },

                ["import"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Go),
                        new LanguageScore(Language.Python),
                        new LanguageScore(Language.Java),
                    },

                ["imports"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.VisualBasic),
                    },

                ["insert"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Sql),
                    },

                ["let"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.TypeScript),
                        new LanguageScore(Language.JavaScript),
                        new LanguageScore(Language.Rust),
                        new LanguageScore(Language.FSharp),
                    },

                ["main"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CPlusPlus),
                        new LanguageScore(Language.C, 0.9),
                        new LanguageScore(Language.CSharp, 0.8),
                    },

                ["member"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.FSharp),
                    },

                ["module"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.VisualBasic),
                        new LanguageScore(Language.FSharp),
                    },

                ["mut"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Rust),
                    },

                ["mutable"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.FSharp),
                    },

                ["namespace"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CSharp),
                        new LanguageScore(Language.CPlusPlus),
                    },

                ["package"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Go),
                        new LanguageScore(Language.Java),
                    },

                ["print"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Python),
                    },

                ["printf"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.C),
                        new LanguageScore(Language.CPlusPlus, 0.9),
                    },

                ["printfn"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.FSharp),
                    },

                ["println"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Rust),
                    },

                ["public"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CSharp),
                        new LanguageScore(Language.VisualBasic),
                        new LanguageScore(Language.Java),
                        new LanguageScore(Language.CPlusPlus),
                    },

                ["raise"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Python),
                    },

                ["range"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Python),
                    },

                ["require"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.JavaScript),
                    },

                ["select"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Sql),
                    },

                ["String"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Java){ExactMatch = "String" },
                    },

                ["sub"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.VisualBasic),
                    },

                ["transient"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Java),
                    },

                ["use"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Rust),
                    },

                ["update"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Sql),
                    },

                ["using"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CSharp),
                        new LanguageScore(Language.CPlusPlus),
                    },

                ["var"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CSharp),
                        new LanguageScore(Language.JavaScript),
                    },

                ["where"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Sql),
                    },

                ["write-host"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.PowerShell),
                    },

                ["write-output"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.PowerShell),
                    },

                ["winmain"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.CSharp),
                        new LanguageScore(Language.CPlusPlus),
                        new LanguageScore(Language.C, 0.9),
                    },

                ["xmlns"] = new List<LanguageScore>
                    {
                        new LanguageScore(Language.Xml),
                    },
            };
            return dic;
        }
    }
}
