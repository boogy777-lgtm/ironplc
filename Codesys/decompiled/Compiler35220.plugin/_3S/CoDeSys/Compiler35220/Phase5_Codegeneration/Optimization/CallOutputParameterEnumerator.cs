using System;
using System.Collections.Generic;
using \u000E;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200025C RID: 604
	public struct CallOutputParameterEnumerator
	{
		// Token: 0x0600274B RID: 10059 RVA: 0x000874F0 File Offset: 0x000856F0
		public CallOutputParameterEnumerator(IList<_IExpression> actualparams, IList<_IExpression> formalparams)
		{
			this.\u0001 = actualparams;
			this.\u0002 = formalparams;
			this.\u0001 = -1;
		}

		// Token: 0x1700070F RID: 1807
		// (get) Token: 0x0600274C RID: 10060 RVA: 0x00087508 File Offset: 0x00085708
		public AssignmentInfo Current
		{
			get
			{
				if (this.\u0001 == null || this.\u0002 == null || this.\u0001 == -1)
				{
					throw new InvalidOperationException();
				}
				_IExpression iexpression = this.\u0002[this.\u0001];
				if (iexpression.Type != null && !TypeTable.IsBlock(iexpression.Type.Class))
				{
					IExpression expression = iexpression;
					\u000F.\u0001(iexpression.Type, iexpression.Type, ref expression);
					iexpression = (expression as _IExpression);
				}
				return new AssignmentInfo(this.\u0001[this.\u0001], iexpression, Operator.AssignOut);
			}
		}

		// Token: 0x0600274D RID: 10061 RVA: 0x0008759C File Offset: 0x0008579C
		public bool MoveNext()
		{
			this.\u0001++;
			return this.\u0001.Count > this.\u0001;
		}

		// Token: 0x0600274E RID: 10062 RVA: 0x000875C0 File Offset: 0x000857C0
		public void Reset()
		{
			this.\u0001 = -1;
		}

		// Token: 0x04000728 RID: 1832
		private readonly IList<_IExpression> \u0001;

		// Token: 0x04000729 RID: 1833
		private readonly IList<_IExpression> \u0002;

		// Token: 0x0400072A RID: 1834
		private int \u0001;
	}
}
