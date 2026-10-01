using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements
{
	// Token: 0x0200026A RID: 618
	public class IfBuilder : IIfBuilder, IIfOptionalPosStep, ILmbPositional<IIfConditionBuilder>, IIfConditionBuilder, IIfThenBuilder, IOptionalIfElseOrElseIfsBuilder, IIfElseBuilder, IIfElseIfsBuilder, ILmbExprementBuilder<IIfStatement>, IOptionalIfElseBuilder
	{
		// Token: 0x060029FE RID: 10750 RVA: 0x0006AE97 File Offset: 0x00069E97
		public IIfConditionBuilder At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x060029FF RID: 10751 RVA: 0x0006AEA1 File Offset: 0x00069EA1
		public IIfThenBuilder Condition(IExpression condition)
		{
			this._ifCondition = condition;
			return this;
		}

		// Token: 0x06002A00 RID: 10752 RVA: 0x0006AEAB File Offset: 0x00069EAB
		public IOptionalIfElseOrElseIfsBuilder Then(ISequenceStatement2 controlled)
		{
			this._then = controlled;
			return this;
		}

		// Token: 0x06002A01 RID: 10753 RVA: 0x0006AEB8 File Offset: 0x00069EB8
		public IIfStatement Build()
		{
			if (this._pos == null)
			{
				this._pos = new ExprementPosition(0L, 0);
			}
			IfStatement ifStatement = new IfStatement();
			ifStatement.SetPositionIntern(MinimalPosition.CreateMinimalPosition(this._pos.Position, this._pos.PositionOffset));
			ifStatement._Condition = (_IExpression)this._ifCondition;
			ifStatement._IfThen = (_IStatement)this._then;
			if (this._else != null)
			{
				ifStatement._IfElse = (_IStatement)this._then;
			}
			if (this._elseIfs != null)
			{
				foreach (IElseIf elseIf in this._elseIfs)
				{
					ifStatement.AddElseIf((_IElseIf)elseIf);
				}
			}
			if (ifStatement._ElseIf.Count <= 0)
			{
				return ifStatement;
			}
			_IStatement ifElse = ifStatement._IfElse;
			for (int i = ifStatement._ElseIf.Count - 1; i >= 0; i--)
			{
				_IElseIf ielseIf = ifStatement._ElseIf[i];
				_IIfStatement iifStatement = LanguageModelBuilder.Singleton.CreateIfStatement();
				iifStatement._Condition = ielseIf._Condition;
				iifStatement._IfThen = ielseIf._Controlled;
				iifStatement._IfElse = ifElse;
				iifStatement._Position = ielseIf._Position;
				iifStatement.SetFlag(StatementFlag.GenerateFlow | StatementFlag.GenerateBP, true);
				_ISequenceStatement isequenceStatement = LanguageModelBuilder.Singleton.CreateSequenceStatement(1);
				isequenceStatement._Position = ielseIf._Position;
				isequenceStatement.Add(iifStatement);
				ifElse = isequenceStatement;
			}
			ifStatement.ClearElseIf();
			ifStatement._IfElse = ifElse;
			return ifStatement;
		}

		// Token: 0x06002A02 RID: 10754 RVA: 0x0006B04C File Offset: 0x0006A04C
		public ILmbExprementBuilder<IIfStatement> Else(ISequenceStatement2 controlled)
		{
			this._else = controlled;
			return this;
		}

		// Token: 0x06002A03 RID: 10755 RVA: 0x0006B056 File Offset: 0x0006A056
		public IOptionalIfElseBuilder ElseIfs(IEnumerable<IElseIf> elseIfs)
		{
			this._elseIfs = new List<IElseIf>();
			this._elseIfs.AddRange(elseIfs);
			return this;
		}

		// Token: 0x06002A04 RID: 10756 RVA: 0x0006B070 File Offset: 0x0006A070
		public static IIfOptionalPosStep Init()
		{
			return new IfBuilder();
		}

		// Token: 0x06002A05 RID: 10757 RVA: 0x0006B077 File Offset: 0x0006A077
		public IIfConditionBuilder NoPosition()
		{
			this._pos = new ExprementPosition(0L, 0);
			return this;
		}

		// Token: 0x040007E4 RID: 2020
		private ISequenceStatement2 _else;

		// Token: 0x040007E5 RID: 2021
		private List<IElseIf> _elseIfs;

		// Token: 0x040007E6 RID: 2022
		private IExpression _ifCondition;

		// Token: 0x040007E7 RID: 2023
		private IExprementPosition _pos;

		// Token: 0x040007E8 RID: 2024
		private ISequenceStatement2 _then;
	}
}
