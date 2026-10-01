using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICrossReferenceService6 : ICrossReferenceService5, ICrossReferenceService4, ICrossReferenceService3, ICrossReferenceService2, ICrossReferenceService
	{
		IList<IRelatedSignature> GetRelatedSignatures(ISignature signature, ISignature subSignature, SignatureRelationFlags flags);
	}
}
