using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000208 RID: 520
	internal class AddressExpression_Green : Expression_Green, _IAddressExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IAddressExpression
	{
		// Token: 0x0600234C RID: 9036 RVA: 0x0005BB00 File Offset: 0x0005AB00
		internal AddressExpression_Green(IDirectVariable dirvar)
		{
			this.m_dirvar = dirvar;
		}

		// Token: 0x170009EA RID: 2538
		// (get) Token: 0x0600234D RID: 9037 RVA: 0x0005BB0F File Offset: 0x0005AB0F
		// (set) Token: 0x0600234E RID: 9038 RVA: 0x0005A471 File Offset: 0x00059471
		public IDirectVariable DirectAddress
		{
			get
			{
				return this.m_dirvar;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600234F RID: 9039 RVA: 0x0000B453 File Offset: 0x0000A453
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002350 RID: 9040 RVA: 0x0000B465 File Offset: 0x0000A465
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006C4 RID: 1732
		private readonly IDirectVariable m_dirvar;
	}
}
