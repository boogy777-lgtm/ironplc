using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ILanguageModelHandling2 : ILanguageModelHandling
	{
		void AddLanguageModelForPOU(_ILanguageModelManagerConsolidated lmm, ILMPOU lmpou, Guid objectGuidLmGlobal, _IPreCompileContext comcon, string stLibraryId);

		void AddLanguageModelForDUT(_ILanguageModelManagerConsolidated lmm, ILMDataType lmdut, Guid objectGuidLmGlobal, _IPreCompileContext comcon, string stLibraryId);

		void AddLanguageModelForGVL(_ILanguageModelManagerConsolidated lmm, ILMGlobVarlist lmgvl, Guid objectGuidLmGlobal, _IPreCompileContext comcon, string stLibraryId);
	}
}
