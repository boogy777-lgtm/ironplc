using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Variable
{
	// Token: 0x020001B2 RID: 434
	public class RarelyUsed
	{
		// Token: 0x04000610 RID: 1552
		internal Dictionary<string, string> m_attributes;

		// Token: 0x04000611 RID: 1553
		internal _IExpression m_initial;

		// Token: 0x04000612 RID: 1554
		internal IDirectVariable m_dirvar;

		// Token: 0x04000613 RID: 1555
		internal IAssignmentExpression[] m_assigns;
	}
}
