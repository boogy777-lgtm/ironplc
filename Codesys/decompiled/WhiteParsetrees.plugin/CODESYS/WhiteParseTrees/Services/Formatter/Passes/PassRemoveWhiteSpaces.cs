using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class PassRemoveWhiteSpaces : IStatementFormattingPass
	{
		private readonly IFormatterSettings _settings;

		private PassRemoveWhiteSpaces(IFormatterSettings settings)
		{
			_settings = settings;
		}

		public void Perform(IWhiteSequenceStatement sequenceStatement)
		{
			WhiteSpaceRemover.RemoveWhiteSpaces(sequenceStatement, _settings);
		}

		public static void Execute(IWhiteSequenceStatement sequenceStatement, IFormatterSettings settings)
		{
			new PassRemoveWhiteSpaces(settings).Perform(sequenceStatement);
		}
	}
}
