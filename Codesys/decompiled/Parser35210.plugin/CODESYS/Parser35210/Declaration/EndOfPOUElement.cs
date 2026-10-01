using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser35210.Declaration
{
	internal class EndOfPOUElement : SyntaxElement
	{
		internal Operator EndOfPOUOperator { get; }

		internal long Position { get; }

		internal short PositionOffset { get; }

		internal EndOfPOUElement(Operator endOfPOUOperator, IToken token)
		{
			EndOfPOUOperator = endOfPOUOperator;
			Position = token.Position;
			PositionOffset = token.PositionOffset;
		}
	}
}
