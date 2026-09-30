using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001D8 RID: 472
	public class CompactedCompiledParseTreeInformation : CompactedParseTreeInformation, ICompactedCompiledParseTreeInformation
	{
		// Token: 0x06002145 RID: 8517 RVA: 0x0005A411 File Offset: 0x00059411
		public override void Clear()
		{
			base.Clear();
			if (this.TypeInfoTable == null)
			{
				this.TypeInfoTable = new LDictionary<int, ICompiledExpressionTypeInfo>();
			}
			this.TypeInfoTable.Clear();
		}

		// Token: 0x17000927 RID: 2343
		// (get) Token: 0x06002146 RID: 8518 RVA: 0x0005A437 File Offset: 0x00059437
		// (set) Token: 0x06002147 RID: 8519 RVA: 0x0005A43F File Offset: 0x0005943F
		public IDictionary<int, ICompiledExpressionTypeInfo> TypeInfoTable { get; set; }
	}
}
