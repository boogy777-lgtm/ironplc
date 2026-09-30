using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPrecompileCrossReferenceService
	{
		IList<ICrossReferenceNode> GetVariableCrossReferences(int signaturePrecompileId, int variablePrecompileId);

		IList<ICrossReferenceNode> GetSignatureCrossReferences(int signaturePrecompileId);

		[Obsolete("This method was never implemented and should not be used")]
		IList<int> GetRelatedSignatures(int signaturePrecompileId, SignatureRelationFlags flags);
	}
}
