using System;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Expressions;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Expressions
{
	// Token: 0x02000270 RID: 624
	public class OperatorBuilder : IOperatorBuilder, IOperatorOptionalPosStep, ILmbPositional<IOperatorLhsBuilder>, IOperatorLhsBuilder, IOperatorOpBuilder, IOperatorRhsBuilder, ILmbExprementBuilder<IOperatorExpression>
	{
		// Token: 0x06002A38 RID: 10808 RVA: 0x0006B496 File Offset: 0x0006A496
		public IOperatorExpression Build()
		{
			if (this._pos == null)
			{
				this._pos = new ExprementPosition(0L, 0);
			}
			return LanguageModelBuilder.Singleton.CreateOperatorExpression(this._pos, this._op, this._lhs, this._rhs);
		}

		// Token: 0x06002A39 RID: 10809 RVA: 0x0006B4D0 File Offset: 0x0006A4D0
		public IOperatorLhsBuilder At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x06002A3A RID: 10810 RVA: 0x0006B4DA File Offset: 0x0006A4DA
		public IOperatorOpBuilder Lhs(IExpression lhs)
		{
			this._lhs = lhs;
			return this;
		}

		// Token: 0x06002A3B RID: 10811 RVA: 0x0006B4E4 File Offset: 0x0006A4E4
		public IOperatorRhsBuilder Op(Operator op)
		{
			this._op = op;
			return this;
		}

		// Token: 0x06002A3C RID: 10812 RVA: 0x0006B4EE File Offset: 0x0006A4EE
		public ILmbExprementBuilder<IOperatorExpression> Rhs(IExpression rhs)
		{
			this._rhs = rhs;
			return this;
		}

		// Token: 0x06002A3D RID: 10813 RVA: 0x0006B4F8 File Offset: 0x0006A4F8
		public static IOperatorOptionalPosStep Init()
		{
			return new OperatorBuilder();
		}

		// Token: 0x040007FF RID: 2047
		private IExpression _lhs;

		// Token: 0x04000800 RID: 2048
		private Operator _op;

		// Token: 0x04000801 RID: 2049
		private IExprementPosition _pos;

		// Token: 0x04000802 RID: 2050
		private IExpression _rhs;
	}
}
