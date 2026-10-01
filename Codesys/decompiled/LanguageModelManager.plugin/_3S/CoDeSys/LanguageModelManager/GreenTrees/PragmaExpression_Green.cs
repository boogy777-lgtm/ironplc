using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000213 RID: 531
	internal abstract class PragmaExpression_Green : Expression_Green, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x17000A0C RID: 2572
		// (get) Token: 0x060023AB RID: 9131 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x060023AC RID: 9132 RVA: 0x0005B849 File Offset: 0x0005A849
		public bool Value
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException("do not attempt to change a green tree");
			}
		}

		// Token: 0x17000A0D RID: 2573
		// (get) Token: 0x060023AD RID: 9133 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x060023AE RID: 9134 RVA: 0x0005B849 File Offset: 0x0005A849
		public bool ValueStillUndecided
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException("do not attempt to change a green tree");
			}
		}
	}
}
