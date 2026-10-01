using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000216 RID: 534
	internal class ProjectDefinedExpression_Green : PragmaExpression_Green, _IProjectDefinedExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x060023B7 RID: 9143 RVA: 0x0005BDC4 File Offset: 0x0005ADC4
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor352000 exprementVisitor = visitor as IExprementVisitor352000;
			if (exprementVisitor != null)
			{
				exprementVisitor.visit(this);
			}
		}

		// Token: 0x060023B8 RID: 9144 RVA: 0x0005BDE4 File Offset: 0x0005ADE4
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor9 exprVisitor = visitor as IExprVisitor9;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x17000A0F RID: 2575
		// (get) Token: 0x060023B9 RID: 9145 RVA: 0x0005BE02 File Offset: 0x0005AE02
		// (set) Token: 0x060023BA RID: 9146 RVA: 0x0005BE0A File Offset: 0x0005AE0A
		public _IDefineReference DefineReference { get; set; }
	}
}
