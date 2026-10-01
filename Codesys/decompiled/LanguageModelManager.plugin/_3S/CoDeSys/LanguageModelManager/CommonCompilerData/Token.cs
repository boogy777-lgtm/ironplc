using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001CA RID: 458
	internal struct Token : _IToken, IToken
	{
		// Token: 0x06002044 RID: 8260 RVA: 0x00059730 File Offset: 0x00058730
		private static Token InitEmpty()
		{
			return new Token
			{
				Type = TokenType.None
			};
		}

		// Token: 0x17000870 RID: 2160
		// (get) Token: 0x06002045 RID: 8261 RVA: 0x0005974E File Offset: 0x0005874E
		// (set) Token: 0x06002046 RID: 8262 RVA: 0x00059756 File Offset: 0x00058756
		public TokenType Type { get; set; }

		// Token: 0x17000871 RID: 2161
		// (get) Token: 0x06002047 RID: 8263 RVA: 0x0005975F File Offset: 0x0005875F
		// (set) Token: 0x06002048 RID: 8264 RVA: 0x00059767 File Offset: 0x00058767
		public int SourceOffset { get; set; }

		// Token: 0x17000872 RID: 2162
		// (get) Token: 0x06002049 RID: 8265 RVA: 0x00059770 File Offset: 0x00058770
		// (set) Token: 0x0600204A RID: 8266 RVA: 0x00059778 File Offset: 0x00058778
		public long Position { get; set; }

		// Token: 0x17000873 RID: 2163
		// (get) Token: 0x0600204B RID: 8267 RVA: 0x00059781 File Offset: 0x00058781
		// (set) Token: 0x0600204C RID: 8268 RVA: 0x00059789 File Offset: 0x00058789
		public short PositionOffset { get; set; }

		// Token: 0x17000874 RID: 2164
		// (get) Token: 0x0600204D RID: 8269 RVA: 0x00059792 File Offset: 0x00058792
		// (set) Token: 0x0600204E RID: 8270 RVA: 0x0005979A File Offset: 0x0005879A
		public int Length { get; set; }

		// Token: 0x17000875 RID: 2165
		// (get) Token: 0x0600204F RID: 8271 RVA: 0x000597A3 File Offset: 0x000587A3
		// (set) Token: 0x06002050 RID: 8272 RVA: 0x000597AB File Offset: 0x000587AB
		public int SourceLine { get; set; }

		// Token: 0x17000876 RID: 2166
		// (get) Token: 0x06002051 RID: 8273 RVA: 0x000597B4 File Offset: 0x000587B4
		// (set) Token: 0x06002052 RID: 8274 RVA: 0x000597BC File Offset: 0x000587BC
		public int SourceColumn { get; set; }

		// Token: 0x17000877 RID: 2167
		// (get) Token: 0x06002053 RID: 8275 RVA: 0x000597C5 File Offset: 0x000587C5
		// (set) Token: 0x06002054 RID: 8276 RVA: 0x000597CD File Offset: 0x000587CD
		public long CharactersToSkipSeen { get; set; }

		// Token: 0x0400065C RID: 1628
		public static readonly Token Empty = Token.InitEmpty();
	}
}
