using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000E2 RID: 226
	public class CodePiece : ICodePiece2, ICodePiece
	{
		// Token: 0x06000FD3 RID: 4051 RVA: 0x0002CBA8 File Offset: 0x0002ADA8
		public CodePiece(int nLength, IDataLocation datalocDestination)
		{
			this.Length = nLength;
			this.Destination = datalocDestination;
		}

		// Token: 0x06000FD4 RID: 4052 RVA: 0x0002CBC0 File Offset: 0x0002ADC0
		public CodePiece(byte[] byCode, IDataLocation datalocDestination)
		{
			this.Code = byCode;
			this.Destination = datalocDestination;
			this.Length = this.Code.Length;
		}

		// Token: 0x1700047A RID: 1146
		// (get) Token: 0x06000FD5 RID: 4053 RVA: 0x0002CBE4 File Offset: 0x0002ADE4
		// (set) Token: 0x06000FD6 RID: 4054 RVA: 0x0002CBEC File Offset: 0x0002ADEC
		public IDataLocation Destination { get; set; }

		// Token: 0x1700047B RID: 1147
		// (get) Token: 0x06000FD7 RID: 4055 RVA: 0x0002CBF8 File Offset: 0x0002ADF8
		// (set) Token: 0x06000FD8 RID: 4056 RVA: 0x0002CC00 File Offset: 0x0002AE00
		public byte[] Code { get; set; }

		// Token: 0x06000FD9 RID: 4057 RVA: 0x0002CC0C File Offset: 0x0002AE0C
		public bool GetFlag(CodePieceFlag flag)
		{
			return ((CodePieceFlag)0 & flag) == flag;
		}

		// Token: 0x1700047C RID: 1148
		// (get) Token: 0x06000FDA RID: 4058 RVA: 0x0002CC14 File Offset: 0x0002AE14
		public int Length { get; }

		// Token: 0x040002B9 RID: 697
		private const CodePieceFlag \u0001 = (CodePieceFlag)0;

		// Token: 0x040002BA RID: 698
		[CompilerGenerated]
		private IDataLocation \u0001;

		// Token: 0x040002BB RID: 699
		[CompilerGenerated]
		private byte[] \u0001;

		// Token: 0x040002BC RID: 700
		[CompilerGenerated]
		private readonly int \u0001;
	}
}
