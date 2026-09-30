using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001C2 RID: 450
	internal class LineScannerContext : ILineScannerContext
	{
		// Token: 0x1700084C RID: 2124
		// (get) Token: 0x06001FE5 RID: 8165 RVA: 0x000589BD File Offset: 0x000579BD
		// (set) Token: 0x06001FE6 RID: 8166 RVA: 0x000589C5 File Offset: 0x000579C5
		public TokenType IncompleteTokenType { get; set; }

		// Token: 0x1700084D RID: 2125
		// (get) Token: 0x06001FE7 RID: 8167 RVA: 0x000589CE File Offset: 0x000579CE
		// (set) Token: 0x06001FE8 RID: 8168 RVA: 0x000589D6 File Offset: 0x000579D6
		public int Depth { get; set; }

		// Token: 0x06001FE9 RID: 8169 RVA: 0x000589E0 File Offset: 0x000579E0
		public override bool Equals(object obj)
		{
			LineScannerContext lineScannerContext = obj as LineScannerContext;
			return lineScannerContext != null && this._incompleteTokenType == lineScannerContext._incompleteTokenType && this._nDepth == lineScannerContext._nDepth;
		}

		// Token: 0x06001FEA RID: 8170 RVA: 0x00058A18 File Offset: 0x00057A18
		public override int GetHashCode()
		{
			return this._incompleteTokenType.GetHashCode() ^ this._nDepth.GetHashCode();
		}

		// Token: 0x04000643 RID: 1603
		private readonly TokenType _incompleteTokenType;

		// Token: 0x04000644 RID: 1604
		private readonly int _nDepth;
	}
}
