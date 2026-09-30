using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x02000017 RID: 23
	public class CompiledExpressionTypeInfo : ICompiledExpressionTypeInfo
	{
		// Token: 0x0600045D RID: 1117 RVA: 0x000092E4 File Offset: 0x000074E4
		public CompiledExpressionTypeInfo(int iSignatureId, int iVariableId, _IType compiledType)
		{
			this.SignatureId = iSignatureId;
			this.VariableId = iVariableId;
			this.CompiledType = compiledType;
		}

		// Token: 0x1700034E RID: 846
		// (get) Token: 0x0600045E RID: 1118 RVA: 0x00009304 File Offset: 0x00007504
		// (set) Token: 0x0600045F RID: 1119 RVA: 0x0000930C File Offset: 0x0000750C
		public int SignatureId { get; set; }

		// Token: 0x1700034F RID: 847
		// (get) Token: 0x06000460 RID: 1120 RVA: 0x00009318 File Offset: 0x00007518
		// (set) Token: 0x06000461 RID: 1121 RVA: 0x00009320 File Offset: 0x00007520
		public int VariableId { get; set; }

		// Token: 0x17000350 RID: 848
		// (get) Token: 0x06000462 RID: 1122 RVA: 0x0000932C File Offset: 0x0000752C
		// (set) Token: 0x06000463 RID: 1123 RVA: 0x00009334 File Offset: 0x00007534
		public _IType CompiledType { get; set; }

		// Token: 0x0400003B RID: 59
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x0400003C RID: 60
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x0400003D RID: 61
		[CompilerGenerated]
		private _IType \u0001;
	}
}
