using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	internal class PassInlineCommentFixes : IStatementFormattingPass
	{
		private PassInlineCommentFixes()
		{
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public void Perform(IWhiteSequenceStatement sequenceStatement)
		{
			InlineCommentFixer.FixInlineComments(sequenceStatement);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public static void Execute(IWhiteSequenceStatement sequenceStatement)
		{
			new PassInlineCommentFixes().Perform(sequenceStatement);
		}
	}
}
