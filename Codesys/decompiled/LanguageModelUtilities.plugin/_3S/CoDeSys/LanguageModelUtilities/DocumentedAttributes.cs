using System;
using System.Collections.Generic;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class DocumentedAttributes : List<Tuple<string, string>>
	{
		public void Add(string attribute, string description)
		{
			Add(new Tuple<string, string>(attribute, description));
		}
	}
}
