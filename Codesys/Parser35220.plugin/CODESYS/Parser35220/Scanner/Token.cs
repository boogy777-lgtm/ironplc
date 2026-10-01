using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Scanner
{
	// Token: 0x02000018 RID: 24
	internal struct Token : _IToken, IToken
	{
		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060001D5 RID: 469 RVA: 0x0000B3D5 File Offset: 0x000095D5
		// (set) Token: 0x060001D6 RID: 470 RVA: 0x0000B3DD File Offset: 0x000095DD
		public TokenType Type { get; set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060001D7 RID: 471 RVA: 0x0000B3E6 File Offset: 0x000095E6
		// (set) Token: 0x060001D8 RID: 472 RVA: 0x0000B3EE File Offset: 0x000095EE
		public int SourceOffset { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060001D9 RID: 473 RVA: 0x0000B3F7 File Offset: 0x000095F7
		// (set) Token: 0x060001DA RID: 474 RVA: 0x0000B3FF File Offset: 0x000095FF
		public long Position { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060001DB RID: 475 RVA: 0x0000B408 File Offset: 0x00009608
		// (set) Token: 0x060001DC RID: 476 RVA: 0x0000B410 File Offset: 0x00009610
		public short PositionOffset { get; set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060001DD RID: 477 RVA: 0x0000B419 File Offset: 0x00009619
		// (set) Token: 0x060001DE RID: 478 RVA: 0x0000B421 File Offset: 0x00009621
		public int Length { get; set; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060001DF RID: 479 RVA: 0x0000B42A File Offset: 0x0000962A
		// (set) Token: 0x060001E0 RID: 480 RVA: 0x0000B432 File Offset: 0x00009632
		public int SourceLine { get; set; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060001E1 RID: 481 RVA: 0x0000B43B File Offset: 0x0000963B
		// (set) Token: 0x060001E2 RID: 482 RVA: 0x0000B443 File Offset: 0x00009643
		public int SourceColumn { get; set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060001E3 RID: 483 RVA: 0x0000B44C File Offset: 0x0000964C
		// (set) Token: 0x060001E4 RID: 484 RVA: 0x0000B454 File Offset: 0x00009654
		public long CharactersToSkipSeen { get; set; }

		// Token: 0x04000053 RID: 83
		public static readonly Token Empty = new Token
		{
			Type = 0
		};
	}
}
