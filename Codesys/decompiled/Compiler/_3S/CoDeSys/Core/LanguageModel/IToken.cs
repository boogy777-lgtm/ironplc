using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IToken
	{
		TokenType Type { get; }

		int SourceOffset { get; }

		long Position { get; }

		short PositionOffset { get; }

		int Length { get; }

		int SourceLine { get; }

		int SourceColumn { get; }
	}
}
