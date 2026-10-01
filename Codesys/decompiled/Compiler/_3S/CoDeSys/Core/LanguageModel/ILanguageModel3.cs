using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModel3 : ILanguageModel2, ILanguageModel
	{
		IEnumerable<ILMLibraryList2> LMLibraryList2 { get; set; }
	}
}
