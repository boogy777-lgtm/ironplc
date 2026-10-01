using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001C3 RID: 451
	internal class LineScannerToken : ILineScannerToken
	{
		// Token: 0x06001FEC RID: 8172 RVA: 0x00058A45 File Offset: 0x00057A45
		public LineScannerToken(TokenType type, int nLineOffset, int nLength)
		{
			this.Type = type;
			this.LineOffset = nLineOffset;
			this.Length = nLength;
		}

		// Token: 0x1700084E RID: 2126
		// (get) Token: 0x06001FED RID: 8173 RVA: 0x00058A62 File Offset: 0x00057A62
		public TokenType Type { get; }

		// Token: 0x1700084F RID: 2127
		// (get) Token: 0x06001FEE RID: 8174 RVA: 0x00058A6A File Offset: 0x00057A6A
		// (set) Token: 0x06001FEF RID: 8175 RVA: 0x00058A72 File Offset: 0x00057A72
		public int LineOffset { get; set; }

		// Token: 0x17000850 RID: 2128
		// (get) Token: 0x06001FF0 RID: 8176 RVA: 0x00058A7B File Offset: 0x00057A7B
		// (set) Token: 0x06001FF1 RID: 8177 RVA: 0x00058A83 File Offset: 0x00057A83
		public int Length { get; set; }
	}
}
