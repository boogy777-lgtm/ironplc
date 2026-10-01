using System;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000270 RID: 624
	public struct CheckFunctionReplacerContext
	{
		// Token: 0x17000727 RID: 1831
		// (get) Token: 0x060027CF RID: 10191 RVA: 0x0008A798 File Offset: 0x00088998
		public CheckFunctions CheckFunctions
		{
			get
			{
				return this.\u0001.Codegeneration.\u0001;
			}
		}

		// Token: 0x17000728 RID: 1832
		// (get) Token: 0x060027D0 RID: 10192 RVA: 0x0008A7B8 File Offset: 0x000889B8
		public IScope5 _Scope
		{
			get
			{
				return this.\u0001._Scope;
			}
		}

		// Token: 0x17000729 RID: 1833
		// (get) Token: 0x060027D1 RID: 10193 RVA: 0x0008A7D4 File Offset: 0x000889D4
		public LateCodeGenerator Generator
		{
			get
			{
				return this.\u0001.Generator;
			}
		}

		// Token: 0x1700072A RID: 1834
		// (get) Token: 0x060027D2 RID: 10194 RVA: 0x0008A7F0 File Offset: 0x000889F0
		public _ICompileContext Comcon
		{
			get
			{
				return this.\u0001.Comcon;
			}
		}

		// Token: 0x1700072B RID: 1835
		// (get) Token: 0x060027D3 RID: 10195 RVA: 0x0008A80C File Offset: 0x00088A0C
		// (set) Token: 0x060027D4 RID: 10196 RVA: 0x0008A814 File Offset: 0x00088A14
		public _ICompiledPOU CompiledPOU { get; set; }

		// Token: 0x060027D5 RID: 10197 RVA: 0x0008A820 File Offset: 0x00088A20
		internal CheckFunctionReplacerContext(\u0011 originalContext, CheckFunctionReplacer replacer)
		{
			this.\u0001 = originalContext;
			this.CompiledPOU = null;
			this.\u0001 = replacer;
		}

		// Token: 0x060027D6 RID: 10198 RVA: 0x0008A838 File Offset: 0x00088A38
		public bool DoChecks()
		{
			return this.\u0001.DoChecks();
		}

		// Token: 0x04000759 RID: 1881
		internal readonly \u0011 \u0001;

		// Token: 0x0400075A RID: 1882
		[CompilerGenerated]
		private _ICompiledPOU \u0001;

		// Token: 0x0400075B RID: 1883
		private readonly CheckFunctionReplacer \u0001;
	}
}
