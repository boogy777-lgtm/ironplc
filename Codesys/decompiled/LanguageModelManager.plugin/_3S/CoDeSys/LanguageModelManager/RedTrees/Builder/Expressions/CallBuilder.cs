using System;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Expressions;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Expressions
{
	// Token: 0x0200026F RID: 623
	public class CallBuilder : ICallBuilder, ICalleeBuilderOptionalPosStep, ILmbPositional<ICalleeBuilder>, ICalleeBuilder, ICalleeOptionalConditionBuilder, ICalleeConditionBuilder, ICalleeParamOrAssignParamPath, ICalleeInputParamBuilder, ICalleeOutputsAssignsBuilder, ILmbExprementBuilder<ICallExpression>, ICalleeInputParamAssignsBuilder
	{
		// Token: 0x06002A2F RID: 10799 RVA: 0x0006B3DB File Offset: 0x0006A3DB
		private CallBuilder()
		{
			this._callExpression = LanguageModelBuilder.Singleton.CreateCallExpression();
		}

		// Token: 0x06002A30 RID: 10800 RVA: 0x0006B3F3 File Offset: 0x0006A3F3
		public ICalleeBuilder At(IExprementPosition position)
		{
			this._callExpression.SetPositionIntern(new MinimalPositionBase(position.Position, position.PositionOffset));
			return this;
		}

		// Token: 0x06002A31 RID: 10801 RVA: 0x0006B417 File Offset: 0x0006A417
		public ICalleeOptionalConditionBuilder Callee(IExpression callee)
		{
			this._callExpression._Callee = (_IExpression)callee;
			return this;
		}

		// Token: 0x06002A32 RID: 10802 RVA: 0x0006B42B File Offset: 0x0006A42B
		public ICalleeParamOrAssignParamPath Condition(IExpression condition)
		{
			this._callExpression._Condition = (_IExpression)condition;
			return this;
		}

		// Token: 0x06002A33 RID: 10803 RVA: 0x0006B43F File Offset: 0x0006A43F
		ICallExpression ILmbExprementBuilder<ICallExpression>.Build()
		{
			return this._callExpression;
		}

		// Token: 0x06002A34 RID: 10804 RVA: 0x0006B447 File Offset: 0x0006A447
		public ICalleeInputParamAssignsBuilder AddParam(IExpression exp, IExpression expVariable)
		{
			this._callExpression.AddParam((_IExpression)exp, (_IExpression)expVariable);
			return this;
		}

		// Token: 0x06002A35 RID: 10805 RVA: 0x0006B461 File Offset: 0x0006A461
		public ICalleeInputParamBuilder AddParam(IExpression exp)
		{
			this._callExpression.AddParam((_IExpression)exp);
			return this;
		}

		// Token: 0x06002A36 RID: 10806 RVA: 0x0006B475 File Offset: 0x0006A475
		public ICalleeOutputsAssignsBuilder AddOutput(IExpression exp, IExpression expVariable)
		{
			this._callExpression.AddOutput((_IExpression)exp, (_IExpression)expVariable);
			return this;
		}

		// Token: 0x06002A37 RID: 10807 RVA: 0x0006B48F File Offset: 0x0006A48F
		public static ICalleeBuilderOptionalPosStep Init()
		{
			return new CallBuilder();
		}

		// Token: 0x040007FE RID: 2046
		private readonly _ICallExpression _callExpression;
	}
}
