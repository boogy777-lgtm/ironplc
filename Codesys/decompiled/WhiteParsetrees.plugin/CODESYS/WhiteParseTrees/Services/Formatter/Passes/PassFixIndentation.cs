using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	internal class PassFixIndentation : IStatementFormattingPass
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public static void Execute(IWhiteSequenceStatement sequenceStatement)
		{
			new PassFixIndentation().Perform(sequenceStatement);
		}

		private PassFixIndentation()
		{
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public void Perform(IWhiteSequenceStatement sequenceStatement)
		{
			TokenSerializer.GetTokenList(sequenceStatement);
		}
	}
}
