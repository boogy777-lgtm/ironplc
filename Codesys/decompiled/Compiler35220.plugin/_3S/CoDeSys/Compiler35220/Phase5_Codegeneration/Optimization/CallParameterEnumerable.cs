using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200025A RID: 602
	public readonly struct CallParameterEnumerable
	{
		// Token: 0x06002745 RID: 10053 RVA: 0x0008742C File Offset: 0x0008562C
		public CallParameterEnumerable(IList<_IExpression> actualparams, IList<_IExpression> formalparams)
		{
			this.\u0001 = actualparams;
			this.\u0002 = formalparams;
		}

		// Token: 0x06002746 RID: 10054 RVA: 0x0008743C File Offset: 0x0008563C
		public CallParameterEnumerator GetEnumerator()
		{
			return new CallParameterEnumerator(this.\u0001, this.\u0002);
		}

		// Token: 0x04000723 RID: 1827
		private readonly IList<_IExpression> \u0001;

		// Token: 0x04000724 RID: 1828
		private readonly IList<_IExpression> \u0002;
	}
}
