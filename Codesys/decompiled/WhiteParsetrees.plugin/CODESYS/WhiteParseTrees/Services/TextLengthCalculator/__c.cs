using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class TextLengthCalculator
	{
		public static int CalculateTextLength(INode node)
		{
			return AddOffsets(TokenSerializer.GetTokenList(node));
		}

		public static int CalculateTextLength(IEnumerable<IWhiteExpression> exprs)
		{
			return exprs.Sum((IWhiteExpression e) => e.GetTextLength());
		}

		public static int CalculateTextLengthWithoutLeadingWhitespace(INode node)
		{
			IList<IWhiteToken> tokenList = TokenSerializer.GetTokenList(node);
			RemoveLeadingWhitespaces(tokenList);
			return AddOffsets(tokenList);
		}

		private static int AddOffsets(IList<IWhiteToken> tokens)
		{
			int num = 0;
			foreach (IWhiteToken token in tokens)
			{
				num += token.Text.Length;
			}
			return num;
		}

		private static void RemoveLeadingWhitespaces(IList<IWhiteToken> tokens)
		{
			while (tokens.FirstOrDefault() != null && tokens.First() is INonSyntacticToken)
			{
				tokens.RemoveAt(0);
			}
		}
	}
}
