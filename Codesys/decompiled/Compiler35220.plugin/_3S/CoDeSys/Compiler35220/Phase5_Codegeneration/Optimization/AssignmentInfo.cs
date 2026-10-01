using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000258 RID: 600
	public readonly struct AssignmentInfo
	{
		// Token: 0x0600273D RID: 10045 RVA: 0x000873A8 File Offset: 0x000855A8
		public AssignmentInfo(_IAssignmentExpression assignmentExpression)
		{
			this.LValue = assignmentExpression._LValue;
			this.RValue = assignmentExpression._RValue;
			this.KindOf = assignmentExpression.KindOf;
		}

		// Token: 0x0600273E RID: 10046 RVA: 0x000873D0 File Offset: 0x000855D0
		public AssignmentInfo(_IExpression lvalue, _IExpression rvalue, Operator kindof)
		{
			this.LValue = lvalue;
			this.RValue = rvalue;
			this.KindOf = kindof;
		}

		// Token: 0x1700070A RID: 1802
		// (get) Token: 0x0600273F RID: 10047 RVA: 0x000873E8 File Offset: 0x000855E8
		public _IExpression LValue { get; }

		// Token: 0x1700070B RID: 1803
		// (get) Token: 0x06002740 RID: 10048 RVA: 0x000873F0 File Offset: 0x000855F0
		public _IExpression RValue { get; }

		// Token: 0x1700070C RID: 1804
		// (get) Token: 0x06002741 RID: 10049 RVA: 0x000873F8 File Offset: 0x000855F8
		public Operator KindOf { get; }

		// Token: 0x1700070D RID: 1805
		// (get) Token: 0x06002742 RID: 10050 RVA: 0x00087400 File Offset: 0x00085600
		public _IExpression PositionExpression
		{
			get
			{
				return this.LValue;
			}
		}

		// Token: 0x0400071E RID: 1822
		[CompilerGenerated]
		private readonly _IExpression \u0001;

		// Token: 0x0400071F RID: 1823
		[CompilerGenerated]
		private readonly _IExpression \u0002;

		// Token: 0x04000720 RID: 1824
		[CompilerGenerated]
		private readonly Operator \u0001;
	}
}
