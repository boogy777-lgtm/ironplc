using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	public interface IStatementFormattingPass
	{
		void Perform(IWhiteSequenceStatement sequenceStatement);
	}
}
