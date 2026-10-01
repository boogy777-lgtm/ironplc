using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements
{
	// Token: 0x02000266 RID: 614
	public class CaseBuilder : ICaseBuilder, ICaseOptionalPosStep, ILmbPositional<ICaseBuilderSwitch>, ICaseBuilderSwitch, ICaseBuilderCase, ICaseBuilderOptionalElse, ILmbExprementBuilder<ICaseStatement>, ICaseBuilderElse
	{
		// Token: 0x060029DD RID: 10717 RVA: 0x0006AB7C File Offset: 0x00069B7C
		public ICaseBuilderSwitch At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x060029DE RID: 10718 RVA: 0x0006AB86 File Offset: 0x00069B86
		public ICaseBuilderCase Switch(IExpression switchExpr)
		{
			this._switch = switchExpr;
			return this;
		}

		// Token: 0x060029DF RID: 10719 RVA: 0x0006AB90 File Offset: 0x00069B90
		public ICaseStatement Build()
		{
			if (this._pos == null)
			{
				this._pos = new ExprementPosition(0L, 0);
			}
			_ICaseStatement icaseStatement = LanguageModelBuilder.Singleton.CreateCaseStatement();
			icaseStatement.SetPositionIntern(new MinimalPositionBase(this._pos.Position, this._pos.PositionOffset));
			icaseStatement._Switch = (_IExpression)this._switch;
			foreach (ICase @case in this._cases)
			{
				icaseStatement.AddCase((_ICase)@case);
			}
			if (this._else != null)
			{
				icaseStatement._Else = (_IStatement)this._else;
			}
			return icaseStatement;
		}

		// Token: 0x060029E0 RID: 10720 RVA: 0x0006AC5C File Offset: 0x00069C5C
		public ICaseBuilderCase Case(ICase caseElement)
		{
			this._cases.Add(caseElement);
			return this;
		}

		// Token: 0x060029E1 RID: 10721 RVA: 0x0006AC6B File Offset: 0x00069C6B
		public ICaseBuilderCase Case(ICaseLabelStatement label, ISequenceStatement2 seq)
		{
			this.Case(LanguageModelBuilder.Singleton.CreateCase(label, seq));
			return this;
		}

		// Token: 0x060029E2 RID: 10722 RVA: 0x0006AC81 File Offset: 0x00069C81
		public ILmbExprementBuilder<ICaseStatement> Else(ISequenceStatement2 controlled)
		{
			this._else = controlled;
			return this;
		}

		// Token: 0x060029E3 RID: 10723 RVA: 0x0006AC8B File Offset: 0x00069C8B
		public static ICaseOptionalPosStep Init()
		{
			return new CaseBuilder();
		}

		// Token: 0x040007D4 RID: 2004
		private readonly List<ICase> _cases = new List<ICase>();

		// Token: 0x040007D5 RID: 2005
		private ISequenceStatement2 _else;

		// Token: 0x040007D6 RID: 2006
		private IExprementPosition _pos;

		// Token: 0x040007D7 RID: 2007
		private IExpression _switch;
	}
}
