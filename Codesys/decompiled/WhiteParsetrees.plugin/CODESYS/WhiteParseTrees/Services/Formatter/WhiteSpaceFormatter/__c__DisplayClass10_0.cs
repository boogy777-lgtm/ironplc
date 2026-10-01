using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Nodes.Factories;

namespace CODESYS.WhiteParseTrees.Services.Formatter
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class WhiteSpaceFormatter
	{
		private static IWhitespaceToken CreateTab()
		{
			return TokenFactory<IWhitespaceToken>.Create("\t");
		}

		private static IWhitespaceToken CreateSpace()
		{
			return TokenFactory<IWhitespaceToken>.Create(" ");
		}

		private static IEndOfLineToken CreateEOL()
		{
			return TokenFactory<IEndOfLineToken>.Create("\r\n");
		}

		public static void AddTab(IWhiteToken token)
		{
			IWhitespaceToken whitespaceToken = CreateTab();
			whitespaceToken.Leading = token.Leading;
			token.Leading = whitespaceToken;
		}

		public static void AddNewLine(IWhiteToken token)
		{
			IEndOfLineToken endOfLineToken = CreateEOL();
			endOfLineToken.Leading = token.Leading;
			token.Leading = endOfLineToken;
		}

		public static void AddSpace(IWhiteToken token)
		{
			IWhitespaceToken whitespaceToken = CreateSpace();
			whitespaceToken.Leading = token.Leading;
			token.Leading = whitespaceToken;
		}

		public static void RemoveLeading(IWhiteToken token)
		{
			if (token.Leading != null)
			{
				token.Leading = token.Leading.Leading;
				if (token.Leading is INonSyntacticToken nonSyntacticToken && nonSyntacticToken.Trailing != null)
				{
					nonSyntacticToken.Trailing = token;
				}
			}
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		public static IWhiteToken GetFirstToken(INode node)
		{
			if (node == null)
			{
				return null;
			}
			while (true)
			{
				if (node is IWhiteToken result)
				{
					return result;
				}
				if (!node.GetChildren().Any())
				{
					break;
				}
				node = node.GetChildren().First();
			}
			return null;
		}

		public static IEnumerable<IWhiteToken> GetAllLeadingTokens(IWhiteToken token)
		{
			List<IWhiteToken> list = new List<IWhiteToken>();
			while (true)
			{
				list.Add(token);
				if (token.Leading == null)
				{
					break;
				}
				token = token.Leading;
			}
			return list;
		}

		public static bool TokenLeadingContains([System.Runtime.CompilerServices.Nullable(2)] IWhiteToken token, Type type)
		{
			if (token == null)
			{
				return false;
			}
			return GetAllLeadingTokens(token).Any(type.IsInstanceOfType);
		}

		public static bool WalkLeadingAndFind(IWhiteToken token, List<Type> targets, Type find)
		{
			foreach (IWhiteToken tok in GetAllLeadingTokens(token))
			{
				if (targets.Any((Type t) => t.IsInstanceOfType(tok)))
				{
					return false;
				}
				if (find.IsInstanceOfType(tok))
				{
					return true;
				}
			}
			return false;
		}
	}
}
