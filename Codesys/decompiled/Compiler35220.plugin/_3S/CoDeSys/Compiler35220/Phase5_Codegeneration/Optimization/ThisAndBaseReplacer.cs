using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u001E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002CF RID: 719
	public class ThisAndBaseReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x17000788 RID: 1928
		// (get) Token: 0x06002B5E RID: 11102 RVA: 0x00098A54 File Offset: 0x00096C54
		private \u0081.\u0010 ReplacerVisitor { get; }

		// Token: 0x17000789 RID: 1929
		// (get) Token: 0x06002B5F RID: 11103 RVA: 0x00098A5C File Offset: 0x00096C5C
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x06002B60 RID: 11104 RVA: 0x00098A64 File Offset: 0x00096C64
		internal ThisAndBaseReplacer(global::\u000E.\u0011 context)
		{
			this.Context = context;
			this.ReplacerVisitor = \u0081.\u0010.\u0003(this, context);
		}

		// Token: 0x06002B61 RID: 11105 RVA: 0x00098A80 File Offset: 0x00096C80
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			if (this.Context._Scope.MethodSignature == null)
			{
				ISignature localSignature = this.Context._Scope.LocalSignature;
				if (localSignature == null || localSignature.POUType != Operator.FunctionBlock)
				{
					return;
				}
			}
			this.ReplacerVisitor.ReplaceCode(cpou);
		}

		// Token: 0x06002B62 RID: 11106 RVA: 0x00098AD4 File Offset: 0x00096CD4
		public override _IExpression ReplaceThisExpression(_IThisExpression thisExpression)
		{
			return \u001E.\u0011.\u0001(thisExpression, this.Context);
		}

		// Token: 0x06002B63 RID: 11107 RVA: 0x00098AE4 File Offset: 0x00096CE4
		public override _IExpression ReplaceBaseExpression(_IBaseExpression baseExpression)
		{
			return \u001E.\u0011.\u0001(baseExpression, this.Context);
		}

		// Token: 0x04000847 RID: 2119
		[CompilerGenerated]
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x04000848 RID: 2120
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;
	}
}
