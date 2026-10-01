using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000E
{
	// Token: 0x020002CA RID: 714
	internal sealed class \u0012 : AbstractToVisitchecker
	{
		// Token: 0x17000781 RID: 1921
		// (get) Token: 0x06002B39 RID: 11065 RVA: 0x000984E0 File Offset: 0x000966E0
		private \u0011 Context { get; }

		// Token: 0x06002B3A RID: 11066 RVA: 0x000984E8 File Offset: 0x000966E8
		internal \u0012(\u0011 \u0083\u0005)
		{
			this.Context = \u0083\u0005;
		}

		// Token: 0x17000782 RID: 1922
		// (get) Token: 0x06002B3B RID: 11067 RVA: 0x000984F8 File Offset: 0x000966F8
		public override bool DoTraversal
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000783 RID: 1923
		// (get) Token: 0x06002B3C RID: 11068 RVA: 0x000984FC File Offset: 0x000966FC
		public override bool VisitNecessary
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000784 RID: 1924
		// (get) Token: 0x06002B3D RID: 11069 RVA: 0x00098500 File Offset: 0x00096700
		// (set) Token: 0x06002B3E RID: 11070 RVA: 0x00098508 File Offset: 0x00096708
		internal bool ToVisitInternal { get; private set; }

		// Token: 0x06002B3F RID: 11071 RVA: 0x00098514 File Offset: 0x00096714
		private bool \u0001(bool \u0002)
		{
			this.ToVisitInternal = (this.ToVisitInternal || \u0002);
			return true;
		}

		// Token: 0x06002B40 RID: 11072 RVA: 0x00098528 File Offset: 0x00096728
		public override bool ToVisit(_ITryCatchStatement trycatch)
		{
			return this.\u0001(true);
		}

		// Token: 0x06002B41 RID: 11073 RVA: 0x00098534 File Offset: 0x00096734
		public override bool ToVisit(_IPragmaStatement pragma)
		{
			return this.\u0001(pragma.Text == "returnlabelposition");
		}

		// Token: 0x06002B42 RID: 11074 RVA: 0x0009854C File Offset: 0x0009674C
		public override bool ToVisit(_IRepeatStatement repeat)
		{
			return this.\u0001(true);
		}

		// Token: 0x06002B43 RID: 11075 RVA: 0x00098558 File Offset: 0x00096758
		public override bool ToVisit(_ICompiledPOU cpou)
		{
			_ISignature isignature = this.Context.Comcon[cpou.SignatureId];
			return isignature.GetFlag(SignatureFlag.RawSTProperty) && isignature.Name.StartsWith("__GET");
		}

		// Token: 0x0400083B RID: 2107
		[CompilerGenerated]
		private readonly \u0011 \u0001;

		// Token: 0x0400083C RID: 2108
		[CompilerGenerated]
		private bool \u0001;
	}
}
