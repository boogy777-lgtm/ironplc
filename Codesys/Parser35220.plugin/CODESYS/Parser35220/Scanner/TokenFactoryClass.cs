using System;
using CODESYS.Parser;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Scanner
{
	// Token: 0x02000017 RID: 23
	public class TokenFactoryClass : ITokenFactory
	{
		// Token: 0x060001D0 RID: 464 RVA: 0x00002076 File Offset: 0x00000276
		private TokenFactoryClass()
		{
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060001D1 RID: 465 RVA: 0x0000B37C File Offset: 0x0000957C
		public static ITokenFactory Singleton
		{
			get
			{
				return TokenFactoryClass.s_singleton;
			}
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x0000B383 File Offset: 0x00009583
		public _IToken CreateEmptyToken()
		{
			return Token.Empty;
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x0000B390 File Offset: 0x00009590
		public _IToken CreateToken(long position, short positionOffset, int length)
		{
			Token empty = Token.Empty;
			empty.Type = 0;
			empty.Position = position;
			empty.PositionOffset = positionOffset;
			empty.Length = length;
			return empty;
		}

		// Token: 0x04000052 RID: 82
		private static readonly TokenFactoryClass s_singleton = new TokenFactoryClass();
	}
}
