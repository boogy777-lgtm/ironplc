using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000218 RID: 536
	internal class TypeReference_Green : ItemReference_Green, _ITypeReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ITypeReference2, ITypeReference
	{
		// Token: 0x060023C2 RID: 9154 RVA: 0x0005BE2A File Offset: 0x0005AE2A
		public TypeReference_Green(_IExpression instancePath)
		{
			this.m_expPath = instancePath;
		}

		// Token: 0x17000A12 RID: 2578
		// (get) Token: 0x060023C3 RID: 9155 RVA: 0x0005BE39 File Offset: 0x0005AE39
		// (set) Token: 0x060023C4 RID: 9156 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x17000A13 RID: 2579
		// (get) Token: 0x060023C5 RID: 9157 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IQualifiedNameExpression Instance
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000A14 RID: 2580
		// (get) Token: 0x060023C6 RID: 9158 RVA: 0x0005BE39 File Offset: 0x0005AE39
		public IExpression InstanceExpression
		{
			get
			{
				return this.m_expPath;
			}
		}

		// Token: 0x060023C7 RID: 9159 RVA: 0x00013000 File Offset: 0x00012000
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060023C8 RID: 9160 RVA: 0x00013012 File Offset: 0x00012012
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006D5 RID: 1749
		private readonly _IExpression m_expPath;
	}
}
