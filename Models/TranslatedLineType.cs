using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4C_Translator
{
	public class TranslatedLineType
	{
		public int Id { get; set; }
		public string Type { get; set; }

		public string Example { get; set; }
		public TranslatedLineType() { }
	}
}
