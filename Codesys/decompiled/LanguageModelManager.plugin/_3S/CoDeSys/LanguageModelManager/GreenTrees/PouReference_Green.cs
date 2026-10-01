using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000219 RID: 537
	internal class PouReference_Green : ItemReference_Green, _IPouReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPouReference2, IPouReference
	{
		// Token: 0x060023C9 RID: 9161 RVA: 0x0005BE41 File Offset: 0x0005AE41
		public PouReference_Green(_IExpression instancePath)
		{
			this.m_expPath = instancePath;
		}

		// Token: 0x17000A15 RID: 2581
		// (get) Token: 0x060023CA RID: 9162 RVA: 0x0005BE50 File Offset: 0x0005AE50
		// (set) Token: 0x060023CB RID: 9163 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x17000A16 RID: 2582
		// (get) Token: 0x060023CC RID: 9164 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IQualifiedNameExpression Instance
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000A17 RID: 2583
		// (get) Token: 0x060023CD RID: 9165 RVA: 0x0005BE50 File Offset: 0x0005AE50
		public IExpression InstanceExpression
		{
			get
			{
				return this.m_expPath;
			}
		}

		// Token: 0x060023CE RID: 9166 RVA: 0x000122E5 File Offset: 0x000112E5
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060023CF RID: 9167 RVA: 0x000122F7 File Offset: 0x000112F7
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006D6 RID: 1750
		private readonly _IExpression m_expPath;
	}
}
