using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager22 : ILanguageModelManager21
	{
		IPreCompileContext SystemContext { get; }

		IEnumerable<IAttribute> RegisteredAttributes { get; }

		void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors, bool forceCompleteLanguageModel);

		bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider);

		IVarRef GetVarReference(Guid guidApplication, string stInstancePath, string stExpression, int nProjectHandle, Guid guidObject);

		ISignature6 GetSignatureForPrecompileID(int precompileId);

		IPreCompileContext GetLibraryPrecompileContext(string stLibraryId);

		void FinishPrecompileChecks();
	}
}
