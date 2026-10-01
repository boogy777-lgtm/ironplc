using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000E
{
	// Token: 0x0200028A RID: 650
	internal struct \u0011
	{
		// Token: 0x1700073B RID: 1851
		// (get) Token: 0x060028D8 RID: 10456 RVA: 0x0008EEB4 File Offset: 0x0008D0B4
		internal IScope5 _Scope { get; }

		// Token: 0x1700073C RID: 1852
		// (get) Token: 0x060028D9 RID: 10457 RVA: 0x0008EEBC File Offset: 0x0008D0BC
		internal LateCodeGenerator Generator { get; }

		// Token: 0x1700073D RID: 1853
		// (get) Token: 0x060028DA RID: 10458 RVA: 0x0008EEC4 File Offset: 0x0008D0C4
		internal _ICompileContext Comcon { get; }

		// Token: 0x1700073E RID: 1854
		// (get) Token: 0x060028DB RID: 10459 RVA: 0x0008EECC File Offset: 0x0008D0CC
		internal Codegeneration Codegeneration { get; }

		// Token: 0x1700073F RID: 1855
		// (get) Token: 0x060028DC RID: 10460 RVA: 0x0008EED4 File Offset: 0x0008D0D4
		// (set) Token: 0x060028DD RID: 10461 RVA: 0x0008EEDC File Offset: 0x0008D0DC
		internal _ICompiledPOU CompiledPOU { get; set; }

		// Token: 0x17000740 RID: 1856
		// (get) Token: 0x060028DE RID: 10462 RVA: 0x0008EEE8 File Offset: 0x0008D0E8
		internal ICodegenerator CodeGen { get; }

		// Token: 0x060028DF RID: 10463 RVA: 0x0008EEF0 File Offset: 0x0008D0F0
		internal \u0011(IScope5 \u009B\u0002, LateCodeGenerator \u0019\u0006, _ICompileContext \u0001\u0002, Codegeneration \u000E\u0002, ICodegenerator \u001A\u0006)
		{
			this._Scope = \u009B\u0002;
			this.Generator = \u0019\u0006;
			this.Comcon = \u0001\u0002;
			this.CompiledPOU = null;
			this.Codegeneration = \u000E\u0002;
			this.CodeGen = \u001A\u0006;
		}

		// Token: 0x04000786 RID: 1926
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x04000787 RID: 1927
		[CompilerGenerated]
		private readonly LateCodeGenerator \u0001;

		// Token: 0x04000788 RID: 1928
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000789 RID: 1929
		[CompilerGenerated]
		private readonly Codegeneration \u0001;

		// Token: 0x0400078A RID: 1930
		[CompilerGenerated]
		private _ICompiledPOU \u0001;

		// Token: 0x0400078B RID: 1931
		[CompilerGenerated]
		private readonly ICodegenerator \u0001;
	}
}
