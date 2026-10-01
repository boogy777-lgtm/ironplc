using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000259 RID: 601
	public readonly struct CallOutputParameterEnumerable
	{
		// Token: 0x06002743 RID: 10051 RVA: 0x00087408 File Offset: 0x00085608
		public CallOutputParameterEnumerable(IList<_IExpression> actualparams, IList<_IExpression> formalparams)
		{
			this.\u0001 = actualparams;
			this.\u0002 = formalparams;
		}

		// Token: 0x06002744 RID: 10052 RVA: 0x00087418 File Offset: 0x00085618
		public CallOutputParameterEnumerator GetEnumerator()
		{
			return new CallOutputParameterEnumerator(this.\u0001, this.\u0002);
		}

		// Token: 0x04000721 RID: 1825
		private readonly IList<_IExpression> \u0001;

		// Token: 0x04000722 RID: 1826
		private readonly IList<_IExpression> \u0002;
	}
}
