using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements
{
	// Token: 0x02000269 RID: 617
	public class ForBuilder : IForBuilder, IForOptionalPosStep, ILmbPositional<IForCounterBuilder>, IForCounterBuilder, IForRangeBuilder, IForByBuilder, IForControlledBuilder, ILmbExprementBuilder<ForStatement>
	{
		// Token: 0x060029F3 RID: 10739 RVA: 0x0006ADC0 File Offset: 0x00069DC0
		public ForStatement Build()
		{
			IAssignmentExpression assCounterStart = LanguageModelBuilder.Singleton.CreateAssignmentExpression(null, this._counterVar, this._counterAssignment);
			return (ForStatement)LanguageModelBuilder.Singleton.CreateForStatement(this._pos, assCounterStart, this._upperbound, this._by, this._controlled);
		}

		// Token: 0x060029F4 RID: 10740 RVA: 0x0006AE0D File Offset: 0x00069E0D
		public IForCounterBuilder At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x060029F5 RID: 10741 RVA: 0x0006AE17 File Offset: 0x00069E17
		public IForRangeBuilder Counter(IVariableExpression counter)
		{
			this._counterVar = counter;
			return this;
		}

		// Token: 0x060029F6 RID: 10742 RVA: 0x0006AE21 File Offset: 0x00069E21
		public IForControlledBuilder By(IExpression by)
		{
			this._by = by;
			return this;
		}

		// Token: 0x060029F7 RID: 10743 RVA: 0x0006AE2B File Offset: 0x00069E2B
		public IForControlledBuilder ByOne()
		{
			this._by = LanguageModelBuilder.Singleton.CreateLiteralExpression(null, 1L);
			return this;
		}

		// Token: 0x060029F8 RID: 10744 RVA: 0x0006AE41 File Offset: 0x00069E41
		public ILmbExprementBuilder<IForStatement> Controlled(ISequenceStatement2 controlled)
		{
			this._controlled = controlled;
			return this;
		}

		// Token: 0x060029F9 RID: 10745 RVA: 0x0006AE4C File Offset: 0x00069E4C
		public ILmbExprementBuilder<IForStatement> Controlled(IEnumerable<IStatement> controlled)
		{
			ISequenceStatement2 controlled2 = LanguageModelBuilder.Singleton.CreateSequenceStatementEx(null, controlled);
			this._controlled = controlled2;
			return this;
		}

		// Token: 0x060029FA RID: 10746 RVA: 0x0006AE6E File Offset: 0x00069E6E
		public IForByBuilder Range(IExpression lowerBound, IExpression upperBound)
		{
			this._counterAssignment = lowerBound;
			this._upperbound = upperBound;
			return this;
		}

		// Token: 0x060029FB RID: 10747 RVA: 0x0006AE7F File Offset: 0x00069E7F
		public static IForOptionalPosStep Init()
		{
			return new ForBuilder();
		}

		// Token: 0x060029FC RID: 10748 RVA: 0x0006AE86 File Offset: 0x00069E86
		public IForCounterBuilder NoPosition()
		{
			this._pos = new ExprementPosition(0L, 0);
			return this;
		}

		// Token: 0x040007DE RID: 2014
		private IExpression _by;

		// Token: 0x040007DF RID: 2015
		private ISequenceStatement2 _controlled;

		// Token: 0x040007E0 RID: 2016
		private IExpression _counterAssignment;

		// Token: 0x040007E1 RID: 2017
		private IVariableExpression _counterVar;

		// Token: 0x040007E2 RID: 2018
		private IExprementPosition _pos;

		// Token: 0x040007E3 RID: 2019
		private IExpression _upperbound;
	}
}
