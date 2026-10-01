using System;
using System.IO;
using System.Runtime.CompilerServices;
using \u0016;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0002
{
	// Token: 0x020003E3 RID: 995
	internal sealed class \u0012
	{
		// Token: 0x17000907 RID: 2311
		// (get) Token: 0x06003774 RID: 14196 RVA: 0x000E4354 File Offset: 0x000E2554
		// (set) Token: 0x06003775 RID: 14197 RVA: 0x000E435C File Offset: 0x000E255C
		private _ICompileContext CompileContext { get; set; }

		// Token: 0x17000908 RID: 2312
		// (get) Token: 0x06003776 RID: 14198 RVA: 0x000E4368 File Offset: 0x000E2568
		// (set) Token: 0x06003777 RID: 14199 RVA: 0x000E4370 File Offset: 0x000E2570
		internal \u0016 BootSorter { get; set; }

		// Token: 0x17000909 RID: 2313
		// (get) Token: 0x06003778 RID: 14200 RVA: 0x000E437C File Offset: 0x000E257C
		// (set) Token: 0x06003779 RID: 14201 RVA: 0x000E4384 File Offset: 0x000E2584
		internal \u0016 Sorter { get; set; }

		// Token: 0x1700090A RID: 2314
		// (get) Token: 0x0600377A RID: 14202 RVA: 0x000E4390 File Offset: 0x000E2590
		private bool BootProject { get; }

		// Token: 0x1700090B RID: 2315
		// (get) Token: 0x0600377B RID: 14203 RVA: 0x000E4398 File Offset: 0x000E2598
		private ICodegenerator Codegenerator
		{
			get
			{
				return this.CompileContext.Codegenerator;
			}
		}

		// Token: 0x0600377C RID: 14204 RVA: 0x000E43A8 File Offset: 0x000E25A8
		internal \u0012(bool \u008E\u0004, _ICompileContext \u0001\u0002)
		{
			this.CompileContext = \u0001\u0002;
			this.BootProject = \u008E\u0004;
			this.BootSorter = null;
			this.Sorter = new \u0016();
		}

		// Token: 0x0600377D RID: 14205 RVA: 0x000E43D0 File Offset: 0x000E25D0
		internal void \u0001(_ICompiledPOU \u0002, Stream \u0003, int \u0004, IRelocation \u0005)
		{
			ICompiledCode compiledCode = \u0002.CompiledCode;
			int num = \u0005.Offset + compiledCode.Location.Offset;
			if (Helper.\u0001(CodegeneratorProperties.WordAddressing, this.Codegenerator))
			{
				num /= 2;
			}
			bool flag = \u0002.GetFlag(CompiledPOUFlags.DataRelocations);
			if (\u0002.GetFlag(CompiledPOUFlags.BootProjectRelevant) && this.BootProject)
			{
				if (this.BootSorter == null)
				{
					this.BootSorter = new \u0016();
				}
				this.BootSorter.\u0001(\u0004, (int)compiledCode.Location.Area, num, flag);
				return;
			}
			this.Sorter.\u0001(\u0004, (int)compiledCode.Location.Area, num, flag);
		}

		// Token: 0x0600377E RID: 14206 RVA: 0x000E4474 File Offset: 0x000E2674
		internal void \u0001()
		{
			\u0016 u = this.Sorter;
			if (u != null)
			{
				u.\u0001();
			}
			\u0016 u2 = this.BootSorter;
			if (u2 == null)
			{
				return;
			}
			u2.\u0001();
		}

		// Token: 0x04000AE4 RID: 2788
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x04000AE5 RID: 2789
		[CompilerGenerated]
		private \u0016 \u0001;

		// Token: 0x04000AE6 RID: 2790
		[CompilerGenerated]
		private \u0016 \u0002;

		// Token: 0x04000AE7 RID: 2791
		[CompilerGenerated]
		private readonly bool \u0001;
	}
}
