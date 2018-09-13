using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace RenderLike.Namegen
{
	/*
	"Configuration": {
		"SyllablePre":  "$P", 
		"SyllableStart": "$s",
		"SyllableMiddle": "$m",
		"SyllableEnd": "$e",
		"SyllablePost": "$p",
		"SyllableVocal": "$v",
		"SyllableConsonant": "$c",
		"SyllablePhoneme": "$?",
		"RuleChancePrefix": "%"
	}
	*/

	public class NameGenerator
	{
		private Rand _rand = new Rand();

		private Dictionary<string,NameGeneratorDefinition> NameGenerators { get; set; }

		public NameGenerator(string nameSetPath)
		{
			NameGenerators = LoadAllNameSets(nameSetPath);
		}

		private Dictionary<string, NameGeneratorDefinition> LoadAllNameSets(string path)
		{
			var dataFiles = Directory.EnumerateFiles(path, "*.json");

			return dataFiles.SelectMany(
				s => JsonConvert.DeserializeObject<IEnumerable<NameGeneratorDefinition>>(File.ReadAllText(s)))
				.ToDictionary(key => key.Name, value => value);
		}

		public string GenerateNameFromSet(string nameSet)
		{
			var parser = new NamegenTokenParser();
			var generator = GetGenerator(nameSet);

			if (generator == null)
				throw new ArgumentException($"Unknown name set '{nameSet}'", nameof(nameSet));

			var sb = new StringBuilder();

			var rules = GetGenerator(nameSet)?.Rules;
			if (rules == null)
				throw new ArgumentException($"No rules defined in nameset '{nameSet}'", nameof(nameSet));

			var parsedRules = rules.Select(s => parser.ParseRule(s)).ToList();


			var rule = _rand.PickWeightedRandom(parsedRules, s => s.RuleChance);

			sb.Append(ParseTokens(nameSet, rule.Tokens));

			return sb.ToString();
		}

		private string ParseTokens(string nameSet, IEnumerable<Token> tokens)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var token in tokens)
			{
				if (token is LiteralToken)
				{
					var literal = token as LiteralToken;
					sb.Append(literal.Literal);
				}
				else
				{
					switch (token.Type)
					{
						case TokenType.Unknown:
							throw new ArgumentOutOfRangeException();
						case TokenType.PreSyllable:
							sb.Append(PickPre(nameSet, token.Chance));
							break;
						case TokenType.StartSyllable:
							sb.Append(PickStart(nameSet, token.Chance));
							break;
						case TokenType.MiddleSyllable:
							sb.Append(PickMiddle(nameSet, token.Chance));
							break;
						case TokenType.EndSyllable:
							sb.Append(PickEnd(nameSet, token.Chance));
							break;
						case TokenType.PostSyllable:
							sb.Append(PickPost(nameSet, token.Chance));
							break;
						case TokenType.Vocal:
							sb.Append(PickVocal(nameSet, token.Chance));
							break;
						case TokenType.Consonant:
							sb.Append(PickConsonant(nameSet, token.Chance));
							break;
						case TokenType.Phoneme:
							sb.Append(PickPhoneme(nameSet, token.Chance));
							break;
						case TokenType.RulePercentagePrefix:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			return sb.ToString();
		}

		string PickPre(string ruleName, float chance) => _rand.PickRandomOrNothing(GetGenerator(ruleName)?.PreSyllables, chance) ?? "";
		string PickStart(string ruleName, float chance) => _rand.PickRandomOrNothing(GetGenerator(ruleName)?.StartSyllables, chance) ?? "";
		string PickMiddle(string ruleName, float chance) => _rand.PickRandomOrNothing(GetGenerator(ruleName)?.MiddleSyllables, chance) ?? "";
		string PickEnd(string ruleName, float chance) => _rand.PickRandomOrNothing(GetGenerator(ruleName)?.EndSyllables, chance) ?? "";
        string PickPost(string ruleName, float chance) => _rand.PickRandomOrNothing(GetGenerator(ruleName)?.PostSyllables, chance) ?? "";
        string PickVocal(string ruleName, float chance) => _rand.PickRandomOrNothing(GetGenerator(ruleName)?.Vocals, chance) ?? "";
        string PickConsonant(string ruleName, float chance) => _rand.PickRandomOrNothing(GetGenerator(ruleName)?.Consonants, chance) ?? "";
        string PickPhoneme(string ruleName, float chance) => _rand.PickRandomOrNothing(GetGenerator(ruleName)?.Phonemes, chance) ?? "";

        private NameGeneratorDefinition GetGenerator(string ruleName)
		{
			NameGeneratorDefinition outValue;
			return NameGenerators.TryGetValue(ruleName, out outValue) ? outValue : null;
		}
	}

	/// <summary>
	/// JSON data container
	/// </summary>
	internal class NameGeneratorDefinition
	{
		public string Name { get; set; }

		public List<string> PreSyllables { get; set; }
		public List<string> StartSyllables { get; set; }
		public List<string> MiddleSyllables { get; set; }
		public List<string> EndSyllables { get; set; }
		public List<string> PostSyllables { get; set; }
		public List<string> Vocals { get; set; }
		public List<string> Consonants { get; set; }
		public List<string> Phonemes { get; set; }
		public List<string> Illegal { get; set; }
		public List<string> Rules { get; set; }
	}
}