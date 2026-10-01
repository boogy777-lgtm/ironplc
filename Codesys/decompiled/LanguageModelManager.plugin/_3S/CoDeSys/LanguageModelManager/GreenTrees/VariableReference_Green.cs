using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000217 RID: 535
	internal class VariableReference_Green : ItemReference_Green, _IVariableReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IVariableReference
	{
		// Token: 0x060023BC RID: 9148 RVA: 0x0005BE13 File Offset: 0x0005AE13
		public VariableReference_Green(_IExpression instancePath)
		{
			this.m_expPath = instancePath;
		}

		// Token: 0x17000A10 RID: 2576
		// (get) Token: 0x060023BD RID: 9149 RVA: 0x0005BE22 File Offset: 0x0005AE22
		// (set) Token: 0x060023BE RID: 9150 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression InstancePath
		{
			get
			{
				return this.m_expPath;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A11 RID: 2577
		// (get) Token: 0x060023BF RID: 9151 RVA: 0x0005BE22 File Offset: 0x0005AE22
		public IExpression Instance
		{
			get
			{
				return this.m_expPath;
			}
		}

		// Token: 0x060023C0 RID: 9152 RVA: 0x00013E73 File Offset: 0x00012E73
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060023C1 RID: 9153 RVA: 0x00013E85 File Offset: 0x00012E85
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006D4 RID: 1748
		private readonly _IExpression m_expPath;
	}
}
