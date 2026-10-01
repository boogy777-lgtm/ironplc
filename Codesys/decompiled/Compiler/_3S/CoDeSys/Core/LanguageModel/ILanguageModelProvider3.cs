using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelProvider3 : ILanguageModelProvider2, ILanguageModelProvider
	{
		string GetLanguageModel2(out List<List<string>> codeTables);
	}
}
