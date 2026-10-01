using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Services
{
	[TypeGuid("4fbad94a-9066-4cf2-9e33-c29142b3c77b")]
	internal class WhiteOffsetService : IWhiteOffsetCalculatorService
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public IDictionary<IWhiteToken, int> CalculateOffsets(INode node)
		{
			return OffsetCalculator.CalculateOffsets(node);
		}
	}
}
