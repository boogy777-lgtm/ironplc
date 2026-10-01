using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements
{
	// Token: 0x02000267 RID: 615
	public class ElseIfBuilder : IElseIfBuilder, IElseIfOptionalPosStep, ILmbPositional<IElseIfConditionBuilder>, IElseIfConditionBuilder, IElseIfControlledBuilder, ILmbExprementBuilder<IElseIf>
	{
		// Token: 0x060029E5 RID: 10725 RVA: 0x0006ACA5 File Offset: 0x00069CA5
		public IElseIf Build()
		{
			if (this._pos == null)
			{
				this._pos = new ExprementPosition(0L, 0);
			}
			return LanguageModelBuilder.Singleton.CreateElseIf(this._pos, this._condition, this._controlled);
		}

		// Token: 0x060029E6 RID: 10726 RVA: 0x0006ACD9 File Offset: 0x00069CD9
		public IElseIfConditionBuilder At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x060029E7 RID: 10727 RVA: 0x0006ACE3 File Offset: 0x00069CE3
		public IElseIfControlledBuilder Condition(IExpression condition)
		{
			this._condition = condition;
			return this;
		}

		// Token: 0x060029E8 RID: 10728 RVA: 0x0006ACED File Offset: 0x00069CED
		public ILmbExprementBuilder<IElseIf> Controlled(ISequenceStatement2 controlled)
		{
			this._controlled = controlled;
			return this;
		}

		// Token: 0x060029E9 RID: 10729 RVA: 0x0006ACF8 File Offset: 0x00069CF8
		public ILmbExprementBuilder<IElseIf> Controlled(IEnumerable<IStatement> controlled)
		{
			ISequenceStatement2 controlled2 = LanguageModelBuilder.Singleton.CreateSequenceStatementEx(null, controlled);
			this._controlled = controlled2;
			return this;
		}

		// Token: 0x060029EA RID: 10730 RVA: 0x0006AD1A File Offset: 0x00069D1A
		public static IElseIfOptionalPosStep Init()
		{
			return new ElseIfBuilder();
		}

		// Token: 0x040007D8 RID: 2008
		private IExpression _condition;

		// Token: 0x040007D9 RID: 2009
		private ISequenceStatement2 _controlled;

		// Token: 0x040007DA RID: 2010
		private IExprementPosition _pos;
	}
}
