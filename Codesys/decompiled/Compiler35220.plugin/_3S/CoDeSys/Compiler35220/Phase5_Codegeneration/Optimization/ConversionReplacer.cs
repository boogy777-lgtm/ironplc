using System;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200029C RID: 668
	public class ConversionReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x17000754 RID: 1876
		// (get) Token: 0x06002A34 RID: 10804 RVA: 0x00093600 File Offset: 0x00091800
		private \u0081.\u0010 ReplacerVisitor { get; }

		// Token: 0x17000755 RID: 1877
		// (get) Token: 0x06002A35 RID: 10805 RVA: 0x00093608 File Offset: 0x00091808
		private ExternalFunctionCallsHandler ExternalFunctionCallsHandler { get; }

		// Token: 0x06002A36 RID: 10806 RVA: 0x00093610 File Offset: 0x00091810
		internal ConversionReplacer(global::\u000E.\u0011 context)
		{
			this.ReplacerVisitor = \u0081.\u0010.\u0001(this, context);
			this.ExternalFunctionCallsHandler = new ExternalFunctionCallsHandler(context.CodeGen, context.Comcon, context.Generator, context._Scope);
		}

		// Token: 0x06002A37 RID: 10807 RVA: 0x0009364C File Offset: 0x0009184C
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001 = cpou;
			this.ReplacerVisitor.ReplaceCode(cpou);
		}

		// Token: 0x06002A38 RID: 10808 RVA: 0x00093664 File Offset: 0x00091864
		public override _IExpression ReplaceConversionExpression(_IConversionExpression conversionExpression)
		{
			return this.ExternalFunctionCallsHandler.HandleExternalFunctionCalls(conversionExpression, this.\u0001);
		}

		// Token: 0x040007BC RID: 1980
		private _ICompiledPOU \u0001;

		// Token: 0x040007BD RID: 1981
		[CompilerGenerated]
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x040007BE RID: 1982
		[CompilerGenerated]
		private readonly ExternalFunctionCallsHandler \u0001;
	}
}
