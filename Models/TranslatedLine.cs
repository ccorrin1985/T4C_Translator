using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace T4C_Translator
{
	public class TranslatedLine
	{
		public string Id { get; set; }
		public string SourceLanguage { get; set; }

		public string OutputLanguage { get; set; }

		public string FullLogText { get; set; }

		public string OriginalText { get; set; }

		public string TranslatedText { get; set; }

		public string OriginalTextXaml { get; set; }

		public string TranslatedTextXaml { get; set; }

		public string Name { get; set; }

		public string Detectedlanguage { get; set; }

		public string Channel { get; set; }

		public string Time { get; set; }

		public TranslatedLineType TranslatedLineType { get; set; }

		public List<Keyword> Keywords { get; set; }

		public List<WordofInterest> WordsofInterest { get; set; }

		public bool IsTranslated { get; set; }

		public bool IsDisplayed { get; set; }

		public string QuickTranslateText
		{
			get
			{
				var returnValue = $"({this.OutputLanguage}: ''{this.OriginalText}'' - {this.SourceLanguage}: ''{this.TranslatedText}'')";
				return returnValue;
			}
		}

		public TranslatedLine() { }
	}
}
