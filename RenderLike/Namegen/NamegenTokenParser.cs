using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderLike.Namegen
{
    public class RuleParseResult
    {
        public IEnumerable<Token> Tokens { get; }
        public float RuleChance { get; }

        public RuleParseResult(IEnumerable<Token> tokens, float ruleChance)
        {
            RuleChance = ruleChance;
            Tokens = tokens;
        }
    }

    public class NamegenTokenParser
    {
        public RuleParseResult ParseRule(string rule)
        {
            var tokens = ParseString(rule).ToList();

            if (tokens.Count(s => s.Type == TokenType.RulePercentagePrefix) > 1)
                throw new ArgumentException($"Rule '{rule}' has more than one rule chance prefix");

            var ruleChance = tokens.SingleOrDefault(s => s.Type == TokenType.RulePercentagePrefix);

            return new RuleParseResult(tokens, ruleChance?.Chance ?? 1.0f);
        }

        private IEnumerable<Token> ParseString(string str) {
            var reader = new StringReader(str);
            bool foundRuleChancePrefix = false;

            while (reader.Peek() != -1) {
                var chr = (char)reader.Peek();

                if ((char)reader.Peek() == '%' && !foundRuleChancePrefix) {
                    reader.Read(); // discard the %
                    foundRuleChancePrefix = true;
                    yield return PickRulePercentage(reader);
                } else if (chr == '$') {
                    reader.Read(); // discard the $
                    yield return pickToken(reader);
                } else if (isLiteralChar(chr)) {
                    yield return PickLiteral(reader);
                } else {
                    break;
                }
            }
        }

        private Token pickToken(StringReader reader)
        {
            var sb = new StringBuilder();
            float percentageValue = 1.0f;
            while (true)
            {
                if (((char)reader.Peek()).isDigit())
                {
                    var chr = (char)reader.Read();
                    sb.Append(chr);
                }
                else
                {
                    var pcrVal = sb.ToString();
                    if (!String.IsNullOrWhiteSpace(pcrVal))
                    {
                        var value = Int32.Parse(pcrVal);
                        percentageValue = value/100f; // Scale 0..1
                    }
                    break;
                }
            }

            var tokenType = (char)reader.Read();
            switch (tokenType)
            {
                case 'P':
                    return new Token(TokenType.PreSyllable, percentageValue);
                case 's':
                    return new Token(TokenType.StartSyllable, percentageValue);
                case 'm':
                    return new Token(TokenType.MiddleSyllable, percentageValue);
                case 'e':
                    return new Token(TokenType.EndSyllable, percentageValue);
                case 'p':
                    return new Token(TokenType.PostSyllable, percentageValue);
                case 'v':
                    return new Token(TokenType.Vocal, percentageValue);
                case 'c':
                    return new Token(TokenType.Consonant, percentageValue);
                case '?':
                    return new Token(TokenType.Phoneme, percentageValue);
                default:
                    return new Token(TokenType.Unknown, 0);
            }
        }


        private Token PickLiteral(StringReader reader)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var peekChr = (char)reader.Peek();
                if (isLiteralChar(peekChr))
                {
                    var chr = (char)reader.Read();
                    sb.Append(chr);
                }
                else if (peekChr == '\\')
                {
                    reader.Read(); // discard the '\'
                    var escapedChar = (char) reader.Read();
                    sb.Append(escapedChar);
                }
                else break;
            }

            return new LiteralToken(sb.ToString(), 0);
        }

        Token PickRulePercentage(StringReader reader)
        {
            var sb = new StringBuilder();
            float percentageValue = 1.0f; // default to 100% chance of picking this rule.
            while (true)
            {
                var peekChr = (char) reader.Peek();
                if (peekChr.isDigit())
                {
                    var chr = (char) reader.Read();
                    sb.Append(chr);
                }
                else
                {
                    var pcrVal = sb.ToString();
                    if (!String.IsNullOrWhiteSpace(pcrVal))
                    {
                        var value = Int32.Parse(pcrVal);
                        percentageValue = value/100f; // Scale 0..1

                    }
                    break;
                }
            }

            return new Token(TokenType.RulePercentagePrefix, percentageValue);
        }
       
        private bool isLiteralChar(char chr) => chr.isAlpha() || chr.isSpace() || chr == '\'' || chr == '-' || chr == '_';
    }

    public enum TokenType
    {
        Unknown,
        RulePercentagePrefix,
        PreSyllable,
        StartSyllable,
        MiddleSyllable,
        EndSyllable,
        PostSyllable,
        Vocal,
        Consonant,
        Phoneme,
        Literal
    }

    public class Token
    {
        public TokenType Type { get; }
        public float Chance { get; }

        public Token(TokenType type, float chance)
        {
            Chance = chance;
            Type = type;
        }
    }

    public class LiteralToken : Token
    {
        public string Literal { get; }

        public LiteralToken(string literal, int chance) : base(TokenType.Literal, chance)
        {
            Literal = literal;
        }
    }
}