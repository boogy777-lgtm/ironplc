using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPrecompileCrossReferenceService2 : IPrecompileCrossReferenceService
	{
		IList<ICrossReferenceNode> GetDirectSignatureCrossReferences(int signaturePrecompileId);
	}
}
