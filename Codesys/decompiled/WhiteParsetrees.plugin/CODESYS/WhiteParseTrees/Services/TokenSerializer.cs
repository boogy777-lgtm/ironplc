using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class TokenSerializer
	{
		public static IList<IWhiteToken> GetTokenList([System.Runtime.CompilerServices.Nullable(2)] INode node)
		{
			List<IWhiteToken> list = new List<IWhiteToken>();
			if (node == null)
			{
				return list;
			}
			CollectTokens(list, node);
			return list;
		}

		private static void GetLeadingTokens(IList<IWhiteToken> tokenList, IWhiteToken token)
		{
			if (token.Leading != null)
			{
				GetLeadingTokens(tokenList, token.Leading);
			}
			tokenList.Add(token);
		}

		private static void CollectTokens(IList<IWhiteToken> tokenList, INode node)
		{
			if (node is IWhiteToken token)
			{
				GetLeadingTokens(tokenList, token);
				return;
			}
			foreach (INode child in node.GetChildren())
			{
				CollectTokens(tokenList, child);
			}
		}
	}
}
