using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPreCompileContext11 : IPreCompileContext10, IPreCompileContext9, IPreCompileContext8, IPreCompileContext7, IPreCompileContext6, IPreCompileContext5, IPreCompileContext4, IPreCompileContext3, IPreCompileContext2, IPreCompileContext, ICompileContextCommon
	{
		bool DeviceApplication { get; }

		IList<IPrecompilePositionInfo> GetVariableReferencePositions(int nSignatureWithReferencesPrecompileId, int nSignatureWithVarPrecompileId, int nVariablePrecompileId);

		IList<IPrecompilePositionInfo> GetCrossReferencePositions(int nCallerSignaturePrecompileId, int nCalleeSignaturePrecompileId);

		void DeriveAccessPathInformation(Guid guidSignature, int nProjectHandle, string stAccessPathOrType, out bool bError, out IPrecompileScope derivedScope, out ISignature derivedSignature, out IVariable derivedVariable, out IType derivedType, out IPrecompileScope searchScope);

		ICollection<IPreCompileContext> LibraryContextsWithResolvedPlaceholders(Guid guidApplication, bool bWithPublishedSymbols);
	}
}
