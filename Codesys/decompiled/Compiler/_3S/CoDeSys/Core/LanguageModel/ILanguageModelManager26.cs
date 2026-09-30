using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager26 : ILanguageModelManager25, ILanguageModelManager24, ILanguageModelManager23, ILanguageModelManager22, ILanguageModelManager21
	{
		event EventHandler<BeforeGenerateRelinkCodeEventArgs> BeforeGenerateRelinkCode;
	}
}
