using System;
using System.Collections.Generic;
using System.Linq;

namespace RenderLike
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

	public class Namegen
	{
		private Rand _rand = new Rand();

		private Dictionary<string,NameGeneratorDefinition> NameGenerators { get; set; }

		public string GenerateFromRule(string ruleName)
		{
			return "";
		}

		private string PickSyllableStart(string ruleName)
		{
		    //return GetGenerator(ruleName).SyllableStart
            throw new NotImplementedException();
		}

		private string PickSyllableMiddle(string ruleName)
		{
            //var pickList = NameGenerators.FirstOrDefault()
            throw new NotImplementedException();
        }

		private string PickSyllableEnd(string ruleName)
		{
            throw new NotImplementedException();
        }

		private NameGeneratorDefinition GetGenerator(string ruleName)
		{
			NameGeneratorDefinition outValue;
			return NameGenerators.TryGetValue(ruleName, out outValue)
				? outValue
				: default(NameGeneratorDefinition);
		}
	}

	/// <summary>
	/// JSON data container
	/// </summary>
	internal class NameGeneratorDefinition
	{
		public List<string> SyllableStart { get; set; }
		public List<string> SyllablesMiddle { get; set; }
		public List<string> SyllablesEnd { get; set; }
		public List<string> Illegal { get; set; }
		public List<string> Rules { get; set; }
	}	
}