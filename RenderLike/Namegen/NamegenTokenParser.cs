using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RenderLike.Namegen
{
    public class NamegenTokenParser
    {
        public IEnumerable<Token> ParseString(string str) {
            var reader = new StringReader(str);

            while (reader.Peek() != -1) {
                var chr = (char)reader.Peek();

                if (chr == '$') {
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
            int percentageValue = 0;
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
                        percentageValue = Int32.Parse(pcrVal);
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

        private bool isLiteralChar(char chr) => chr.isAlpha() || chr.isSpace() || chr == '\'' || chr == '-' || chr == '_';
    }

    public enum TokenType
    {
        Unknown,
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
        public double Chance { get; }

        public Token(TokenType type, double chance)
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