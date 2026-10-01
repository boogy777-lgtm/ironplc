using System.Linq;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class InlineCommentFixer
	{
		private InlineCommentFixer()
		{
		}

		public static void FixInlineComments(INode node)
		{
			new InlineCommentFixer().TraverseTokens(node);
		}

		private void TraverseTokens(INode node)
		{
			if (node is IWhiteToken whiteToken && (whiteToken is ICommentToken || whiteToken is IDocCommentToken) && !(whiteToken.Leading is IWhitespaceToken) && !(whiteToken.Leading is IEndOfLineToken))
			{
				WhiteSpaceFormatter.AddSpace(whiteToken);
			}
			if (node is ICommentToken commentToken && commentToken.Comment.Contains("*"))
			{
				commentToken.Comment = commentToken.Comment.Replace("\t", "");
				commentToken.Text = commentToken.Text.Replace("\t", "");
				AddIndentToMultilineComment(WhiteSpaceFormatter.GetAllLeadingTokens(commentToken).TakeWhile((IWhiteToken e) => !(e is IEndOfLineToken)).Count((IWhiteToken e) => e is IWhitespaceToken && e.Text == "\t"), commentToken);
			}
			foreach (INode child in node.GetChildren())
			{
				TraverseTokens(child);
			}
		}

		private static void AddIndentToMultilineComment(int nNumOfTabs, ICommentToken tokenToFormat)
		{
			string text = new string('\t', nNumOfTabs);
			tokenToFormat.Text = tokenToFormat.Text.Replace("\n", "\n" + text);
			tokenToFormat.Comment = tokenToFormat.Comment.Replace("\n", "\n" + text);
		}
	}
}
