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

        private string GenerateFromRule(string ruleName)
        {
            return "";
        }

        private string PickSyllableStart(string ruleName)
        {
            return GetGenerator(ruleName).SyllableStart.Random()
        }

        private string PickSyllableMiddle(string ruleName)
        {
            var pickList = NameGenerators.FirstOrDefault()
        }

        private string PickSyllableEnd(string ruleName)
        {
            
        }

        private NameGeneratorDefinition GetGenerator(string ruleName)
        {
            NameGeneratorDefinition outValue;
            return NameGenerators.TryGetValue(ruleName, out outValue)
                ? outValue
                : default(NameGeneratorDefinition);
        }
    }

    internal static class EnumerableExtensions
    {
        public T Random()
        {
            
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