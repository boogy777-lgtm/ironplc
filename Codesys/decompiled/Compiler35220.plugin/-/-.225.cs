using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0018
{
	// Token: 0x02000269 RID: 617
	internal sealed class \u0007 : AbstractToVisitchecker
	{
		// Token: 0x060027A6 RID: 10150 RVA: 0x000897A0 File Offset: 0x000879A0
		internal \u0007(CallReplacer \u0018\u0006, Func<_ISignature, _ICallExpression, bool>[] \u0013\u0006)
		{
			this.Replacer = \u0018\u0006;
			this.Checkers = \u0013\u0006;
		}

		// Token: 0x1700071F RID: 1823
		// (get) Token: 0x060027A7 RID: 10151 RVA: 0x000897B8 File Offset: 0x000879B8
		// (set) Token: 0x060027A8 RID: 10152 RVA: 0x000897C0 File Offset: 0x000879C0
		private Func<_ISignature, _ICallExpression, bool>[] Checkers { get; set; }

		// Token: 0x17000720 RID: 1824
		// (get) Token: 0x060027A9 RID: 10153 RVA: 0x000897CC File Offset: 0x000879CC
		private CallReplacer Replacer { get; }

		// Token: 0x060027AA RID: 10154 RVA: 0x000897D4 File Offset: 0x000879D4
		public override bool ToVisit(_ICallExpression call)
		{
			_ISignature arg = this.Replacer.\u0001(call);
			Func<_ISignature, _ICallExpression, bool>[] array = this.Checkers;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i](arg, call))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x04000737 RID: 1847
		[CompilerGenerated]
		private Func<_ISignature, _ICallExpression, bool>[] \u0001;

		// Token: 0x04000738 RID: 1848
		[CompilerGenerated]
		private readonly CallReplacer \u0001;
	}
}
