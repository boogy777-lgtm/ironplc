using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPrecompileCrossReferenceService3 : IPrecompileCrossReferenceService2, IPrecompileCrossReferenceService
	{
		IList<ICrossReferenceNode> GetSignatureCrossReferences(int signaturePrecompileId, SignatureCrossReferenceFlags flags);
	}
}
