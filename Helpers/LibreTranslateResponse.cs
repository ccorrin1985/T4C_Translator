using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4C_Translator
{
	public class TranslationResponse
	{
		[JsonProperty("alternatives")]
		public List<string> Alternatives { get; set; }

		[JsonProperty("detectedLanguage")]
		public DetectedLanguage DetectedLanguage { get; set; }

		[JsonProperty("translatedText")]
		public string TranslatedText { get; set; }
	}

	public class DetectedLanguage
	{
		[JsonProperty("confidence")]
		public string Confidence { get; set; }

		[JsonProperty("language")]
		public string Language { get; set; }
	}
}