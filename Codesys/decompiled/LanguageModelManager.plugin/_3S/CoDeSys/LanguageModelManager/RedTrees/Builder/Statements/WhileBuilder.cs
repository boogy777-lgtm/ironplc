using System;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements
{
	// Token: 0x0200026E RID: 622
	public class WhileBuilder : IWhileBuilder, IWhileOptionalPosStep, ILmbPositional<IWhileConditionBuilder>, IWhileConditionBuilder, IWhileControlledBuilder, ILmbExprementBuilder<IWhileStatement>
	{
		// Token: 0x06002A29 RID: 10793 RVA: 0x0006B382 File Offset: 0x0006A382
		public IWhileStatement Build()
		{
			if (this._pos == null)
			{
				this._pos = new ExprementPosition(0L, 0);
			}
			return LanguageModelBuilder.Singleton.CreateWhileStatement(this._pos, this._condition, this._controlled);
		}

		// Token: 0x06002A2A RID: 10794 RVA: 0x0006B3B6 File Offset: 0x0006A3B6
		public IWhileConditionBuilder At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x06002A2B RID: 10795 RVA: 0x0006B3C0 File Offset: 0x0006A3C0
		public IWhileControlledBuilder Condition(IExpression condition)
		{
			this._condition = condition;
			return this;
		}

		// Token: 0x06002A2C RID: 10796 RVA: 0x0006B3CA File Offset: 0x0006A3CA
		public ILmbExprementBuilder<IWhileStatement> Controlled(ISequenceStatement2 controlled)
		{
			this._controlled = controlled;
			return this;
		}

		// Token: 0x06002A2D RID: 10797 RVA: 0x0006B3D4 File Offset: 0x0006A3D4
		public static IWhileOptionalPosStep Init()
		{
			return new WhileBuilder();
		}

		// Token: 0x040007FB RID: 2043
		private IExpression _condition;

		// Token: 0x040007FC RID: 2044
		private ISequenceStatement2 _controlled;

		// Token: 0x040007FD RID: 2045
		private IExprementPosition _pos;
	}
}
