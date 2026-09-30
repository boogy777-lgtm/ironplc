using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IIsUpTopDateStrategy
	{
		bool IsUpToDate { get; }

		bool OnlineChangePossible { get; }

		bool FastOnlineChangePossible { get; }

		IList<string> Changes { get; }

		IList<IChangedLMObject> DetailedChanges { get; }

		void CheckNextPOU();

		void CheckGlobalOnlineChangePreConditions();

		bool SimulationModeChanged();

		bool DefinesChanged();

		bool CompileOptionsChanged();

		bool ParentContextChanged();

		bool ParentContextNull();

		bool MemorySettingsChanged();

		bool LibraryParamTablesChanged();

		bool LibraryListChanged();

		bool GlobalError();

		bool ExternalSignatureFlagChanged();

		bool SignatureChanged(_ISignature sign, _ISignature signPrecom);

		bool SignatureChangedByChecksumAttribute(_ISignature sign, _ISignature signPrecom);

		bool SubSignaturesChanged(_ISignature sign);

		bool CompiledPouChanged(_ISignature sign, _ICompiledPOU cpou, _ICompiledPOU cpouPrecomp);

		bool AdditionalSignInPrecompile(_ISignature sign, _ISignature signCompiled);

		bool AdditionalSignInPool(_ISignature signPool, _ISignature signCompiled);

		bool PrecomNameHashChanged();
	}
}
