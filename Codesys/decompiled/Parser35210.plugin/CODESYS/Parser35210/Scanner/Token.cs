using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35210.Scanner
{
	internal struct Token : _IToken, IToken
	{
		public static readonly Token Empty = new Token
		{
			Type = TokenType.None
		};

		public TokenType Type { get; set; }

		public int SourceOffset { get; set; }

		public long Position { get; set; }

		public short PositionOffset { get; set; }

		public int Length { get; set; }

		public int SourceLine { get; set; }

		public int SourceColumn { get; set; }

		public long CharactersToSkipSeen { get; set; }
	}
}
