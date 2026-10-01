using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteOffsetCalculatorService
	{
		IDictionary<IWhiteToken, int> CalculateOffsets(INode node);
	}
}
