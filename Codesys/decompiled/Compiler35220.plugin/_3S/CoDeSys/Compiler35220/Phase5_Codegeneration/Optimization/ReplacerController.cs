using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200027E RID: 638
	public class ReplacerController
	{
		// Token: 0x17000733 RID: 1843
		// (get) Token: 0x06002837 RID: 10295 RVA: 0x0008BBBC File Offset: 0x00089DBC
		private IReplacer Replacer { get; }

		// Token: 0x17000734 RID: 1844
		// (get) Token: 0x06002838 RID: 10296 RVA: 0x0008BBC4 File Offset: 0x00089DC4
		public IToVisitchecker Checker { get; }

		// Token: 0x17000735 RID: 1845
		// (get) Token: 0x06002839 RID: 10297 RVA: 0x0008BBCC File Offset: 0x00089DCC
		private bool VisitNecessary
		{
			get
			{
				return this.Checker == null || this.Checker.VisitNecessary;
			}
		}

		// Token: 0x0600283A RID: 10298 RVA: 0x0008BBE4 File Offset: 0x00089DE4
		public ReplacerController(IReplacer replacer, IToVisitchecker checker)
		{
			this.Replacer = replacer;
			this.Checker = checker;
		}

		// Token: 0x0600283B RID: 10299 RVA: 0x0008BBFC File Offset: 0x00089DFC
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			if (this.VisitNecessary)
			{
				this.Replacer.ReplaceCode(cpou);
			}
		}

		// Token: 0x0400076E RID: 1902
		[CompilerGenerated]
		private readonly IReplacer \u0001;

		// Token: 0x0400076F RID: 1903
		[CompilerGenerated]
		private readonly IToVisitchecker \u0001;
	}
}
