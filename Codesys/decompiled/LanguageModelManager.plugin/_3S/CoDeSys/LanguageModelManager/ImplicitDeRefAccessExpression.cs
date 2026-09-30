using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200005C RID: 92
	[TypeGuid("{ACA97D61-D3BA-4674-8538-3D8D9315A346}")]
	[StorageVersion("3.3.0.0")]
	public class ImplicitDeRefAccessExpression : DeRefAccessExpression, _IImplicitDeRefAccessExpression, _IDeRefAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IDeRefAccessExpression
	{
		// Token: 0x06000571 RID: 1393 RVA: 0x0000F329 File Offset: 0x0000E329
		public ImplicitDeRefAccessExpression()
		{
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0000F331 File Offset: 0x0000E331
		internal ImplicitDeRefAccessExpression(_IExpression exp) : base(exp)
		{
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x0000F33A File Offset: 0x0000E33A
		internal ImplicitDeRefAccessExpression(_IExpression exp, IToken token) : base(exp, token)
		{
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x0000F344 File Offset: 0x0000E344
		public override _IExprement Duplicate()
		{
			ImplicitDeRefAccessExpression implicitDeRefAccessExpression = new ImplicitDeRefAccessExpression();
			if (this.m_exp != null)
			{
				implicitDeRefAccessExpression.m_exp = (this.m_exp.Duplicate() as Expression);
			}
			this.DuplicateCommon(implicitDeRefAccessExpression);
			return implicitDeRefAccessExpression;
		}
	}
}
