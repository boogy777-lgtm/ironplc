using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter
{
	[System.Runtime.CompilerServices.NullableContext(2)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class FormatUtil
	{
		public static void CheckedAddSpace(INode node)
		{
			if (node == null)
			{
				return;
			}
			IWhiteToken firstToken = WhiteSpaceFormatter.GetFirstToken(node);
			if (firstToken == null)
			{
				return;
			}
			if (node is IWhiteCommentStatement whiteCommentStatement && whiteCommentStatement.CommentToken.IsBlockComment)
			{
				WhiteSpaceFormatter.AddSpace(whiteCommentStatement.CommentToken);
				return;
			}
			List<IWhiteToken> list = WhiteSpaceFormatter.GetAllLeadingTokens(firstToken).ToList();
			if (list.Count > 1)
			{
				IWhiteToken whiteToken = list[1];
				if ((whiteToken is ICommentToken commentToken && !commentToken.IsBlockComment) || whiteToken is IDocCommentToken)
				{
					IWhiteToken token = list.First();
					WhiteSpaceFormatter.AddNewLine(token);
					WhiteSpaceFormatter.AddTab(token);
					WhiteSpaceFormatter.AddSpace(whiteToken);
					return;
				}
				if (whiteToken is ICommentToken commentToken2 && commentToken2.IsBlockComment)
				{
					WhiteSpaceFormatter.AddSpace(commentToken2);
				}
			}
			IWhiteToken firstToken2 = WhiteSpaceFormatter.GetFirstToken(node);
			if (firstToken2 != null)
			{
				WhiteSpaceFormatter.AddSpace(firstToken2);
			}
		}

		public static void CheckedAddIndent(INode node, int nNumOfTabs)
		{
			if (node == null)
			{
				return;
			}
			IWhiteToken whiteToken = null;
			if (node is IWhiteToken whiteToken2)
			{
				whiteToken = whiteToken2;
			}
			else if (node.GetChildren().Any())
			{
				whiteToken = WhiteSpaceFormatter.GetFirstToken(node);
			}
			if (whiteToken == null)
			{
				return;
			}
			if (whiteToken is ICommentToken commentToken && commentToken.IsBlockComment)
			{
				AddIndentToMultilineComment(nNumOfTabs, commentToken);
			}
			if (whiteToken is ICommentToken || whiteToken is IDocCommentToken)
			{
				HandleInlineComment(nNumOfTabs, whiteToken);
				return;
			}
			for (int i = 0; i < nNumOfTabs; i++)
			{
				WhiteSpaceFormatter.AddTab(whiteToken);
			}
		}

		public static void CheckedAddLineAndIndent(INode node, int nNumOfTabs)
		{
			if (node == null)
			{
				return;
			}
			IWhiteToken whiteToken = null;
			if (node is IWhiteToken whiteToken2)
			{
				whiteToken = whiteToken2;
			}
			else if (node.GetChildren().Any())
			{
				whiteToken = WhiteSpaceFormatter.GetFirstToken(node);
			}
			if (whiteToken == null)
			{
				return;
			}
			if (whiteToken is ICommentToken commentToken && commentToken.IsBlockComment)
			{
				AddIndentToMultilineComment(nNumOfTabs, commentToken);
			}
			List<IWhiteToken> list = WhiteSpaceFormatter.GetAllLeadingTokens(whiteToken).ToList();
			foreach (IWhiteToken item in list.Skip(1))
			{
				IndentLeadingComment(nNumOfTabs, item);
			}
			if (list[0] is ICommentToken || list[0] is IDocCommentToken)
			{
				HandleInlineComment(nNumOfTabs, whiteToken);
				return;
			}
			WhiteSpaceFormatter.AddNewLine(whiteToken);
			for (int i = 0; i < nNumOfTabs; i++)
			{
				WhiteSpaceFormatter.AddTab(whiteToken);
			}
		}

		public static void CheckedAddLine(INode node)
		{
			if (node != null)
			{
				IWhiteToken whiteToken = null;
				if (node is IWhiteToken whiteToken2)
				{
					whiteToken = whiteToken2;
				}
				else if (node.GetChildren().Any())
				{
					whiteToken = WhiteSpaceFormatter.GetFirstToken(node);
				}
				if (whiteToken != null)
				{
					WhiteSpaceFormatter.AddNewLine(whiteToken);
				}
			}
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		private static void AddIndentToMultilineComment(int nNumOfTabs, ICommentToken tokenToFormat)
		{
			string text = new string('\t', nNumOfTabs);
			tokenToFormat.Text = tokenToFormat.Text.Replace("\n", "\n" + text);
			tokenToFormat.Comment = tokenToFormat.Comment.Replace("\n", "\n" + text);
		}

		private static void IndentLeadingComment(int nNumOfTabs, INode tokenToFormat)
		{
			IWhiteToken whiteToken = ((tokenToFormat is ICommentToken commentToken) ? ((INonSyntacticToken)commentToken) : ((INonSyntacticToken)((!(tokenToFormat is IDocCommentToken docCommentToken)) ? null : docCommentToken)));
			IWhiteToken whiteToken2 = whiteToken;
			if (whiteToken2 != null && WhiteSpaceFormatter.TokenLeadingContains(whiteToken2, typeof(IEndOfLineToken)))
			{
				for (int i = 0; i < nNumOfTabs; i++)
				{
					WhiteSpaceFormatter.AddTab(whiteToken2);
				}
			}
		}

		private static void HandleInlineComment(int nNumOfTabs, INode tokenToFormat)
		{
			if (tokenToFormat == null)
			{
				return;
			}
			if (tokenToFormat is IWhiteToken token && !WhiteSpaceFormatter.TokenLeadingContains(token, typeof(IEndOfLineToken)))
			{
				CheckedAddSpace(tokenToFormat);
				return;
			}
			for (int i = 0; i < nNumOfTabs; i++)
			{
				IWhiteToken firstToken = WhiteSpaceFormatter.GetFirstToken(tokenToFormat);
				if (firstToken == null)
				{
					break;
				}
				WhiteSpaceFormatter.AddTab(firstToken);
			}
		}
	}
}
