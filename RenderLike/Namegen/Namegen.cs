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

		public NameGenerator(string rulePath)
		{
			NameGenerators = LoadAllRules(rulePath);
		}

		private Dictionary<string, NameGeneratorDefinition> LoadAllRules(string path)
		{
			var dataFiles = Directory.EnumerateFiles(path, "*.json");

			return dataFiles.SelectMany(
				s => JsonConvert.DeserializeObject<IEnumerable<NameGeneratorDefinition>>(File.ReadAllText(s)))
				.ToDictionary(key => key.Name, value => value);
		}

		public string GenerateFromRule(string ruleName)
		{
			var parser = new NamegenTokenParser();
			var generator = GetGenerator(ruleName);

			if (generator == null)
				throw new ArgumentException($"Unknown rule '{ruleName}'", nameof(ruleName));

			var rule = PickRandom(GetGenerator(ruleName)?.Rules);
			var sb = new StringBuilder();

			foreach (var token in parser.ParseString(rule))
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
							sb.Append(PickPre(ruleName));
							break;
						case TokenType.StartSyllable:
							sb.Append(PickStart(ruleName));
							break;
						case TokenType.MiddleSyllable:
							sb.Append(PickMiddle(ruleName));
							break;
						case TokenType.EndSyllable:
							sb.Append(PickEnd(ruleName));
							break;
						case TokenType.PostSyllable:
							sb.Append(PickPost(ruleName));
							break;
						case TokenType.Vocal:
							sb.Append(PickVocal(ruleName));
							break;
						case TokenType.Consonant:
							sb.Append(PickConsonant(ruleName));
							break;
						case TokenType.Phoneme:
							sb.Append(PickPhoneme(ruleName));
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			return sb.ToString();
		}

		string PickPre(string ruleName) => PickRandom(GetGenerator(ruleName)?.PreSyllables);
		string PickStart(string ruleName) => PickRandom(GetGenerator(ruleName)?.StartSyllables);
		string PickMiddle(string ruleName) => PickRandom(GetGenerator(ruleName)?.MiddleSyllables);
		string PickEnd(string ruleName) => PickRandom(GetGenerator(ruleName)?.EndSyllables);
		string PickPost(string ruleName) => PickRandom(GetGenerator(ruleName)?.PostSyllables);
		string PickVocal(string ruleName) => PickRandom(GetGenerator(ruleName)?.Vocals);
		string PickConsonant(string ruleName) => PickRandom(GetGenerator(ruleName)?.Consonants);
		string PickPhoneme(string ruleName) => PickRandom(GetGenerator(ruleName)?.Phonemes);

		private NameGeneratorDefinition GetGenerator(string ruleName)
		{
			NameGeneratorDefinition outValue;
			return NameGenerators.TryGetValue(ruleName, out outValue) ? outValue : null;
		}

		private T PickRandom<T>(IList<T> sequence)
		{
		    if (sequence != null && sequence.Any())
		        return sequence.ElementAt(_rand.GetInt(0, sequence.Count - 1));

		    throw new ArgumentNullException(nameof(sequence), "Sequence referenced with no values");
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