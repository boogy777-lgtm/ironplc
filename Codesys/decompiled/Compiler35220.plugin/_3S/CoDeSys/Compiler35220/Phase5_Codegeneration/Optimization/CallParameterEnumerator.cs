using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200025B RID: 603
	public struct CallParameterEnumerator
	{
		// Token: 0x06002747 RID: 10055 RVA: 0x00087450 File Offset: 0x00085650
		public CallParameterEnumerator(IList<_IExpression> actualparams, IList<_IExpression> formalparams)
		{
			this.\u0001 = actualparams;
			this.\u0002 = formalparams;
			this.\u0001 = -1;
		}

		// Token: 0x1700070E RID: 1806
		// (get) Token: 0x06002748 RID: 10056 RVA: 0x00087468 File Offset: 0x00085668
		public AssignmentInfo Current
		{
			get
			{
				if (this.\u0001 == null || this.\u0002 == null || this.\u0001 == -1)
				{
					throw new InvalidOperationException();
				}
				return new AssignmentInfo(this.\u0002[this.\u0001], this.\u0001[this.\u0001], Operator.Assign);
			}
		}

		// Token: 0x06002749 RID: 10057 RVA: 0x000874C0 File Offset: 0x000856C0
		public bool MoveNext()
		{
			this.\u0001++;
			return this.\u0001.Count > this.\u0001;
		}

		// Token: 0x0600274A RID: 10058 RVA: 0x000874E4 File Offset: 0x000856E4
		public void Reset()
		{
			this.\u0001 = -1;
		}

		// Token: 0x04000725 RID: 1829
		private readonly IList<_IExpression> \u0001;

		// Token: 0x04000726 RID: 1830
		private readonly IList<_IExpression> \u0002;

		// Token: 0x04000727 RID: 1831
		private int \u0001;
	}
}
