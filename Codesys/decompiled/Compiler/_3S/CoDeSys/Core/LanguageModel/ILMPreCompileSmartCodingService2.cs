using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileSmartCodingService2 : ILMPreCompileSmartCodingService
	{
		IntellisenseScopeInfoFlags GetIntellisenseScopeInfoForItem(IIdentifierInfo iiItem);

		IntellisenseScopeInfoFlags GetIntellisenseScopeInfoForItem(IVariable var, ISignature sig, ItemTypeFlags itemType);

		IEnumerable<string> GetConversionOperators();

		IEnumerable<string> GetOverloadedConversionOperators();
	}
}
