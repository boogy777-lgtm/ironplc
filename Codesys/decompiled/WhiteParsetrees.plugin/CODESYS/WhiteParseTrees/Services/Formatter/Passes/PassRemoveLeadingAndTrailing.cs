using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class PassRemoveLeadingAndTrailing : IStatementFormattingPass
	{
		public static void Execute(IWhiteSequenceStatement sequenceStatement)
		{
			new PassRemoveLeadingAndTrailing().Perform(sequenceStatement);
		}

		private PassRemoveLeadingAndTrailing()
		{
		}

		public void Perform(IWhiteSequenceStatement sequenceStatement)
		{
			if (sequenceStatement.Count > 1)
			{
				IWhiteToken whiteToken = WhiteSpaceFormatter.GetFirstToken(sequenceStatement[0]);
				while (whiteToken?.Leading?.Leading != null)
				{
					whiteToken = whiteToken.Leading;
				}
				if (whiteToken != null)
				{
					WhiteSpaceFormatter.RemoveLeading(whiteToken);
				}
				TraverseNodes(sequenceStatement);
			}
		}

		private void TraverseNodes(INode node)
		{
			if (node is IWhiteToken token)
			{
				CheckTokenAndLeading(token);
			}
			foreach (INode child in node.GetChildren())
			{
				TraverseNodes(child);
			}
		}

		private void CheckTokenAndLeading(IWhiteToken token)
		{
			IWhiteToken leading = token.Leading;
			if (leading is IWhitespaceToken && leading.Text == " ")
			{
				leading = token.Leading?.Leading;
				if (leading is IWhitespaceToken && leading.Text == "\t")
				{
					WhiteSpaceFormatter.RemoveLeading(token);
				}
			}
		}
	}
}
