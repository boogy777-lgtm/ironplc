using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.WhiteParseTrees.Extensions
{
	public static class WhiteTokenExtensions
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public static IEnumerable<IWhiteToken> LeadingTokensRToL(this IWhiteToken token)
		{
			for (IWhiteToken current = token; current != null; current = current.Leading)
			{
				yield return current;
			}
		}
	}
}
