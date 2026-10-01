using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000215 RID: 533
	internal class DefineReference_Green : ItemReference_Green, _IDefineReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IDefineReference
	{
		// Token: 0x060023B2 RID: 9138 RVA: 0x0005BDAC File Offset: 0x0005ADAC
		public DefineReference_Green(string stDefine)
		{
			this.m_stDefine = stDefine;
		}

		// Token: 0x17000A0E RID: 2574
		// (get) Token: 0x060023B3 RID: 9139 RVA: 0x0005BDBB File Offset: 0x0005ADBB
		// (set) Token: 0x060023B4 RID: 9140 RVA: 0x0005A471 File Offset: 0x00059471
		public string Define
		{
			get
			{
				return this.m_stDefine;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x060023B5 RID: 9141 RVA: 0x0000E38B File Offset: 0x0000D38B
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060023B6 RID: 9142 RVA: 0x0000E39D File Offset: 0x0000D39D
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006D2 RID: 1746
		private readonly string m_stDefine;
	}
}
