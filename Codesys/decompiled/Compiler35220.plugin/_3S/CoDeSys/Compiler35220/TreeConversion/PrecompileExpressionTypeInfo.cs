using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x02000016 RID: 22
	public class PrecompileExpressionTypeInfo : IPrecompileTypeInfo
	{
		// Token: 0x06000458 RID: 1112 RVA: 0x000092A4 File Offset: 0x000074A4
		public PrecompileExpressionTypeInfo(int iSignatureId, int iVariableId)
		{
			this.PrecompileSignatureId = iSignatureId;
			this.PrecompileVariableId = iVariableId;
		}

		// Token: 0x1700034C RID: 844
		// (get) Token: 0x06000459 RID: 1113 RVA: 0x000092BC File Offset: 0x000074BC
		// (set) Token: 0x0600045A RID: 1114 RVA: 0x000092C4 File Offset: 0x000074C4
		public int PrecompileSignatureId { get; set; }

		// Token: 0x1700034D RID: 845
		// (get) Token: 0x0600045B RID: 1115 RVA: 0x000092D0 File Offset: 0x000074D0
		// (set) Token: 0x0600045C RID: 1116 RVA: 0x000092D8 File Offset: 0x000074D8
		public int PrecompileVariableId { get; set; }

		// Token: 0x04000039 RID: 57
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x0400003A RID: 58
		[CompilerGenerated]
		private int \u0002;
	}
}
