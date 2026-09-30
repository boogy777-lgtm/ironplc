using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILanguageModelManagerConsolidated2 : _ILanguageModelManagerConsolidated, _ILanguageModelManagerLegacy
	{
		void StoreCompileContextWithStackOverflow(Guid guidApplication, _ICompileContext comcon);

		void RemoveCompiledApplicationSetWithStackoverflow(Guid guidApplication);

		_ICompileContext GetCompileContextWithStackOverflow(Guid guidApplication);
	}
}
