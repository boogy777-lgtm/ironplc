using System.Collections.Generic;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileCrossReferenceService
	{
		bool PrecompileInformationUpToDate { get; }

		void FinishPrecompileChecks();

		void EnablePrecompileChecksInNoUIMode();

		void DisablePrecompileChecksInNoUIMode();

		bool CheckAllPOUs(ILMPreCompileSet precompileSet);

		IEnumerable<IAccessInfo> GetDirectVariableAccess(IDirectVariable dirvar);

		IEnumerable<IDirectVariableAccess> GetAllDirectVariableAccesses();

		IEnumerable<IAccessInfo> GetVariableAccess(string stVariableName);

		IEnumerable<IAccessInfo> GetPOUAccess(string stPOUName);

		IDictionary<string, IList<IAccessInfo>> GetPrecompiledCrossReferences(Regex regex);

		IList<IPrecompilePositionInfo> GetVariableReferencePositions(ILMPreCompileSet setOfSignature, int nSignatureWithReferencesPrecompileId, int nSignatureWithVarPrecompileId, int nVariablePrecompileId);

		IList<IPrecompilePositionInfo> GetCrossReferencePositions(ILMPreCompileSet setOfSignature, int nCallerSignaturePrecompileId, int nCalleeSignaturePrecompileId);

		IList<IPrecompilePositionInfo> GetDirectCrossReferencePositions(ILMPreCompileSet setOfSignature, int nCallerSignaturePrecompileId, int nCalleeSignaturePrecompileId);
	}
}
