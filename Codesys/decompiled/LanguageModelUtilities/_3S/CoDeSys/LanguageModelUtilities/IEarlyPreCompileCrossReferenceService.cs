using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IEarlyPreCompileCrossReferenceService
	{
		void ForceVariableCrossReferences(int signaturePrecompileId, int variablePrecompileId);

		void ForceSignatureCrossReferences(int signaturePrecompileId);

		void ForceSymbolCrossReferences(string stSymbol);

		IList<ICrossReferenceNode> GetEarlyVariableCrossReferences(int signaturePrecompileId, int variablePrecompileId);

		IList<ICrossReferenceNode> GetEarlyDirectSignatureCrossReferences(int signaturePrecompileId);

		IList<ICrossReferenceNode> GetEarlySignatureCrossReferences(int signaturePrecompileId, SignatureCrossReferenceFlags flags);
	}
}
