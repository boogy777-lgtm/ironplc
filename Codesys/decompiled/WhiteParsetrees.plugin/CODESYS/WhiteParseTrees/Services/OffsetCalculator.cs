using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	public static class OffsetCalculator
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public static IDictionary<IWhiteToken, int> CalculateOffsets(INode node)
		{
			int num = 0;
			IDictionary<IWhiteToken, int> dictionary = new Dictionary<IWhiteToken, int>();
			foreach (IWhiteToken token in TokenSerializer.GetTokenList(node))
			{
				dictionary.Add(token, num);
				num += token.Text.Length;
			}
			return dictionary;
		}
	}
}
