using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager30 : ILanguageModelManager29, ILanguageModelManager28, ILanguageModelManager27, ILanguageModelManager26, ILanguageModelManager25, ILanguageModelManager24, ILanguageModelManager23, ILanguageModelManager22, ILanguageModelManager21
	{
		bool IsUpToDate(Guid guidApplication);
	}
}
