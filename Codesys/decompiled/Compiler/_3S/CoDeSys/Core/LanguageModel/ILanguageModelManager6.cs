using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager6 : ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		event CompileEventHandler TaskConfigChanged;

		event SignatureChangedEventHandler SignatureChanged;

		event SignatureChangedEventHandler SignatureDeleted;

		event SignatureChangedEventHandler SignatureInserted;

		event CompiledPOUChangedEventHandler CompiledPOUChanged;

		event CompiledPOUChangedEventHandler CompiledPOUDeleted;

		event CompiledPOUChangedEventHandler CompiledPOUInserted;

		Guid[] GetSubApplicationGuids(Guid guidApplication, bool bRecursive);

		Guid GetParentApplicationGuid(Guid guidApplication);
	}
}
