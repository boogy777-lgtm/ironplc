using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200022B RID: 555
	internal class PragmaIfStatement_Green : Statement_Green, _IPragmaIfStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaIfStatement
	{
		// Token: 0x06002456 RID: 9302 RVA: 0x0005C2EA File Offset: 0x0005B2EA
		internal PragmaIfStatement_Green(_IExpression expCond, _IStatement ifthen, _IStatement ifelse, _IPragmaElseIf[] elsifs)
		{
			this.m_expCond = expCond;
			this.m_smIfThen = ifthen;
			this.m_smIfElse = ifelse;
			this.m_ElseIfs = elsifs;
		}

		// Token: 0x17000A48 RID: 2632
		// (get) Token: 0x06002457 RID: 9303 RVA: 0x0005C30F File Offset: 0x0005B30F
		// (set) Token: 0x06002458 RID: 9304 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression Condition
		{
			get
			{
				if (this.m_expCond == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expCond;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A49 RID: 2633
		// (get) Token: 0x06002459 RID: 9305 RVA: 0x0005C325 File Offset: 0x0005B325
		// (set) Token: 0x0600245A RID: 9306 RVA: 0x0005A471 File Offset: 0x00059471
		public _IStatement IfThen
		{
			get
			{
				if (this.m_smIfThen == null)
				{
					return new NullStatement_Green();
				}
				return this.m_smIfThen;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A4A RID: 2634
		// (get) Token: 0x0600245B RID: 9307 RVA: 0x0005C33B File Offset: 0x0005B33B
		// (set) Token: 0x0600245C RID: 9308 RVA: 0x0005A471 File Offset: 0x00059471
		public _IStatement IfElse
		{
			get
			{
				return this.m_smIfElse;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A4B RID: 2635
		// (get) Token: 0x0600245D RID: 9309 RVA: 0x0005C343 File Offset: 0x0005B343
		public IList<_IPragmaElseIf> ElseIf
		{
			get
			{
				if (this.m_ElseIfs == null)
				{
					return Array.Empty<_IPragmaElseIf>();
				}
				return this.m_ElseIfs;
			}
		}

		// Token: 0x0600245E RID: 9310 RVA: 0x0005A471 File Offset: 0x00059471
		public void AddElseIf(_IPragmaElseIf elseIf)
		{
			throw new NotSupportedException();
		}

		// Token: 0x0600245F RID: 9311 RVA: 0x000161CA File Offset: 0x000151CA
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002460 RID: 9312 RVA: 0x000161DC File Offset: 0x000151DC
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x17000A4C RID: 2636
		// (get) Token: 0x06002461 RID: 9313 RVA: 0x0005C35C File Offset: 0x0005B35C
		public IElseIf[] ElseIfs
		{
			get
			{
				if (this.m_ElseIfs == null)
				{
					return Array.Empty<IElseIf>();
				}
				IElseIf[] array = new IElseIf[this.m_ElseIfs.Length];
				this.m_ElseIfs.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x17000A4D RID: 2637
		// (get) Token: 0x06002462 RID: 9314 RVA: 0x0005C393 File Offset: 0x0005B393
		public IStatement IfElseStatement
		{
			get
			{
				return this.IfElse;
			}
		}

		// Token: 0x17000A4E RID: 2638
		// (get) Token: 0x06002463 RID: 9315 RVA: 0x0005C39B File Offset: 0x0005B39B
		public IExpression ConditionExpression
		{
			get
			{
				return this.Condition;
			}
		}

		// Token: 0x17000A4F RID: 2639
		// (get) Token: 0x06002464 RID: 9316 RVA: 0x0005C3A3 File Offset: 0x0005B3A3
		public IStatement IfThenStatement
		{
			get
			{
				return this.IfThen;
			}
		}

		// Token: 0x040006F4 RID: 1780
		private readonly _IExpression m_expCond;

		// Token: 0x040006F5 RID: 1781
		private readonly _IStatement m_smIfThen;

		// Token: 0x040006F6 RID: 1782
		private readonly _IStatement m_smIfElse;

		// Token: 0x040006F7 RID: 1783
		private readonly _IPragmaElseIf[] m_ElseIfs;
	}
}
