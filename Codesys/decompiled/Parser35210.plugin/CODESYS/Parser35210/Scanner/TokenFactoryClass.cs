using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;

namespace CODESYS.Parser35210.Scanner
{
	public class TokenFactoryClass : ITokenFactory
	{
		private static readonly TokenFactoryClass s_singleton = new TokenFactoryClass();

		public static ITokenFactory Singleton => s_singleton;

		private TokenFactoryClass()
		{
		}

		public _IToken CreateEmptyToken()
		{
			return Token.Empty;
		}

		public _IToken CreateToken(long position, short positionOffset, int length)
		{
			Token empty = Token.Empty;
			empty.Type = TokenType.None;
			empty.Position = position;
			empty.PositionOffset = positionOffset;
			empty.Length = length;
			return empty;
		}
	}
}
