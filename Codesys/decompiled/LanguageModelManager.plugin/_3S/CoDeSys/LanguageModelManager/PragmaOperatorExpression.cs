using System;
using System.Collections.Generic;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200006C RID: 108
	[TypeGuid("{1866a46b-2adf-44c8-a1ea-9248ec3ee20d}")]
	[StorageVersion("3.3.0.0")]
	public class PragmaOperatorExpression : PragmaExpression, _IPragmaOperatorExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPragmaOperatorExpression
	{
		// Token: 0x060006F5 RID: 1781 RVA: 0x000123EC File Offset: 0x000113EC
		public PragmaOperatorExpression()
		{
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x00012400 File Offset: 0x00011400
		public PragmaOperatorExpression(PragmaOperator op, IToken token) : base(token)
		{
			this.m_op = op;
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x0001241C File Offset: 0x0001141C
		public PragmaOperatorExpression(PragmaOperator op)
		{
			this.m_op = op;
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x060006F8 RID: 1784 RVA: 0x00012437 File Offset: 0x00011437
		// (set) Token: 0x060006F9 RID: 1785 RVA: 0x0001243F File Offset: 0x0001143F
		public PragmaOperator Code
		{
			get
			{
				return this.m_op;
			}
			set
			{
				this.m_op = value;
			}
		}

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x060006FA RID: 1786 RVA: 0x00012448 File Offset: 0x00011448
		// (set) Token: 0x060006FB RID: 1787 RVA: 0x00012483 File Offset: 0x00011483
		public Operator Operator
		{
			get
			{
				switch (this.m_op)
				{
				case PragmaOperator.Or:
					return Operator.Or;
				case PragmaOperator.And:
					return Operator.And;
				case PragmaOperator.Not:
					return Operator.Not;
				default:
					return Operator.None;
				}
			}
			set
			{
				if (value == Operator.And)
				{
					this.m_op = PragmaOperator.And;
					return;
				}
				if (value == Operator.Or)
				{
					this.m_op = PragmaOperator.Or;
					return;
				}
				if (value != Operator.Not)
				{
					this.m_op = PragmaOperator.None;
					return;
				}
				this.m_op = PragmaOperator.Not;
			}
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x060006FC RID: 1788 RVA: 0x000124BE File Offset: 0x000114BE
		public IList<_IExpression> Operands
		{
			get
			{
				return this.m_alOperands;
			}
		}

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x060006FD RID: 1789 RVA: 0x000124C8 File Offset: 0x000114C8
		public IExpression[] AllOperands
		{
			get
			{
				_IExpression[] array = new _IExpression[this.m_alOperands.Count];
				this.m_alOperands.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x000124F6 File Offset: 0x000114F6
		public void AddOperand(_IExpression exp)
		{
			this.m_alOperands.Add(exp);
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x00012504 File Offset: 0x00011504
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x0001250D File Offset: 0x0001150D
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x00012516 File Offset: 0x00011516
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000703 RID: 1795 RVA: 0x00012520 File Offset: 0x00011520
		public override _IExprement Duplicate()
		{
			PragmaOperatorExpression pragmaOperatorExpression = new PragmaOperatorExpression();
			this.DuplicateCommon(pragmaOperatorExpression);
			pragmaOperatorExpression.m_op = this.m_op;
			foreach (_IExpression iexpression in this.m_alOperands)
			{
				pragmaOperatorExpression.AddOperand(iexpression.Duplicate() as _IExpression);
			}
			return pragmaOperatorExpression;
		}

		// Token: 0x040000EC RID: 236
		[DefaultSerialization("Operands")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alOperands = new LList<_IExpression>(2);

		// Token: 0x040000ED RID: 237
		[DefaultSerialization("Operator")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private PragmaOperator m_op;
	}
}
