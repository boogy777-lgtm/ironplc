using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001D7 RID: 471
	public class CompactedPrecompileParseTreeInformation : CompactedParseTreeInformation, ICompactedParseTreeInformation2, ICompactedParseTreeInformation
	{
		// Token: 0x0600213F RID: 8511 RVA: 0x0005A398 File Offset: 0x00059398
		public override void Clear()
		{
			base.Clear();
			if (this.TypeInfoTable == null)
			{
				this.TypeInfoTable = new LDictionary<int, IPrecompileTypeInfo>();
			}
			this.TypeInfoTable.Clear();
			if (this.StatementFlagTable == null)
			{
				this.StatementFlagTable = new LDictionary<int, StatementFlag>();
			}
			this.StatementFlagTable.Clear();
		}

		// Token: 0x17000925 RID: 2341
		// (get) Token: 0x06002140 RID: 8512 RVA: 0x0005A3E7 File Offset: 0x000593E7
		// (set) Token: 0x06002141 RID: 8513 RVA: 0x0005A3EF File Offset: 0x000593EF
		public IDictionary<int, IPrecompileTypeInfo> TypeInfoTable { get; set; }

		// Token: 0x17000926 RID: 2342
		// (get) Token: 0x06002142 RID: 8514 RVA: 0x0005A3F8 File Offset: 0x000593F8
		// (set) Token: 0x06002143 RID: 8515 RVA: 0x0005A400 File Offset: 0x00059400
		public IDictionary<int, StatementFlag> StatementFlagTable { get; set; }
	}
}
