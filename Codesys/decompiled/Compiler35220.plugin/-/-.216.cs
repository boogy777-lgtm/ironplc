using System;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x02000257 RID: 599
	internal sealed class \u0008 : AbstractToVisitchecker
	{
		// Token: 0x0600273A RID: 10042 RVA: 0x000872D0 File Offset: 0x000854D0
		internal \u0008(\u0011 \u0083\u0005, Func<_IAssignmentExpression, \u0011, bool>[] \u0013\u0006)
		{
			this.\u0001 = \u0083\u0005;
			this.\u0001 = \u0013\u0006;
		}

		// Token: 0x0600273B RID: 10043 RVA: 0x000872E8 File Offset: 0x000854E8
		public override bool ToVisit(_IExpressionStatement expstat)
		{
			_IAssignmentExpression iassignmentExpression = expstat._Expr as _IAssignmentExpression;
			if (iassignmentExpression != null)
			{
				Func<_IAssignmentExpression, \u0011, bool>[] u = this.\u0001;
				for (int i = 0; i < u.Length; i++)
				{
					if (u[i](iassignmentExpression, this.\u0001))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600273C RID: 10044 RVA: 0x00087330 File Offset: 0x00085530
		public override bool ToVisit(_IStructureInitialization structureInitialization)
		{
			foreach (_IAssignmentExpression arg in structureInitialization._CompoInits)
			{
				Func<_IAssignmentExpression, \u0011, bool>[] u = this.\u0001;
				for (int i = 0; i < u.Length; i++)
				{
					if (u[i](arg, this.\u0001))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0400071C RID: 1820
		private readonly \u0011 \u0001;

		// Token: 0x0400071D RID: 1821
		private readonly Func<_IAssignmentExpression, \u0011, bool>[] \u0001;
	}
}
