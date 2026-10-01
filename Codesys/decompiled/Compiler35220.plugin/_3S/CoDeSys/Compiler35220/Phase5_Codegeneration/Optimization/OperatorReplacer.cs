using System;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200029B RID: 667
	public class OperatorReplacer : AbstractReplacer, IReplacer, IExprementReplacer
	{
		// Token: 0x1700074F RID: 1871
		// (get) Token: 0x06002A2A RID: 10794 RVA: 0x00093494 File Offset: 0x00091694
		private \u0081.\u0010 ReplacerVisitor { get; }

		// Token: 0x17000750 RID: 1872
		// (get) Token: 0x06002A2B RID: 10795 RVA: 0x0009349C File Offset: 0x0009169C
		private ExternalFunctionCallsHandler ExternalFunctionCallsHandler { get; }

		// Token: 0x17000751 RID: 1873
		// (get) Token: 0x06002A2C RID: 10796 RVA: 0x000934A4 File Offset: 0x000916A4
		private DateTimeConversionHandler DateTimeConversionHandler { get; }

		// Token: 0x17000752 RID: 1874
		// (get) Token: 0x06002A2D RID: 10797 RVA: 0x000934AC File Offset: 0x000916AC
		private OperationsReplacer OperationsReplacer { get; }

		// Token: 0x17000753 RID: 1875
		// (get) Token: 0x06002A2E RID: 10798 RVA: 0x000934B4 File Offset: 0x000916B4
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x06002A2F RID: 10799 RVA: 0x000934BC File Offset: 0x000916BC
		internal OperatorReplacer(global::\u000E.\u0011 context)
		{
			this.Context = context;
			this.ReplacerVisitor = \u0081.\u0010.\u0001(this, context);
			this.ExternalFunctionCallsHandler = new ExternalFunctionCallsHandler(context.CodeGen, context.Comcon, context.Generator, context._Scope);
			this.DateTimeConversionHandler = new DateTimeConversionHandler(context.Generator, context._Scope);
			this.OperationsReplacer = new OperationsReplacer(this.ExternalFunctionCallsHandler, context);
		}

		// Token: 0x06002A30 RID: 10800 RVA: 0x00093538 File Offset: 0x00091738
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001 = cpou;
			this.ReplacerVisitor.ReplaceCode(cpou);
			this.OperationsReplacer.ReplaceCode(cpou);
		}

		// Token: 0x06002A31 RID: 10801 RVA: 0x0009355C File Offset: 0x0009175C
		public void ReplaceExprement(_IExprement exprement, _ICompiledPOU cpou)
		{
			this.OperationsReplacer.ReplaceCodeInExprement(cpou, exprement);
		}

		// Token: 0x06002A32 RID: 10802 RVA: 0x0009356C File Offset: 0x0009176C
		public override _IExpression ReplaceConversionExpression(_IConversionExpression conversionExpression)
		{
			if (conversionExpression.From == TypeClass.Bool && conversionExpression.To == TypeClass.Bool)
			{
				return conversionExpression._Exp;
			}
			_IExpression result;
			if (this.DateTimeConversionHandler.TryConvertConversion(conversionExpression, this.\u0001, out result))
			{
				return result;
			}
			return conversionExpression;
		}

		// Token: 0x06002A33 RID: 10803 RVA: 0x000935AC File Offset: 0x000917AC
		public override _IExpression ReplaceCurrentTaskExpression(_ICurrentTaskExpression currentTaskExpression)
		{
			return this.Context.Generator.GenerateExpression(string.Format("{0}^.{1}", IdentifierConstants.CurrentTaskInfoPointer, currentTaskExpression._Base), this.Context._Scope, this.Context.CompiledPOU);
		}

		// Token: 0x040007B6 RID: 1974
		private _ICompiledPOU \u0001;

		// Token: 0x040007B7 RID: 1975
		[CompilerGenerated]
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x040007B8 RID: 1976
		[CompilerGenerated]
		private readonly ExternalFunctionCallsHandler \u0001;

		// Token: 0x040007B9 RID: 1977
		[CompilerGenerated]
		private readonly DateTimeConversionHandler \u0001;

		// Token: 0x040007BA RID: 1978
		[CompilerGenerated]
		private readonly OperationsReplacer \u0001;

		// Token: 0x040007BB RID: 1979
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;
	}
}
