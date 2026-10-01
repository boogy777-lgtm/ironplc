using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001D6 RID: 470
	public abstract class CompactedParseTreeInformation
	{
		// Token: 0x06002137 RID: 8503 RVA: 0x0005A335 File Offset: 0x00059335
		protected CompactedParseTreeInformation()
		{
			this.Clear();
		}

		// Token: 0x06002138 RID: 8504 RVA: 0x0005A343 File Offset: 0x00059343
		public virtual void Clear()
		{
			if (this.MessageTable == null)
			{
				this.MessageTable = new LDictionary<int, IList<_ICompilerMessage>>();
			}
			this.MessageTable.Clear();
		}

		// Token: 0x17000922 RID: 2338
		// (get) Token: 0x06002139 RID: 8505 RVA: 0x0005A363 File Offset: 0x00059363
		// (set) Token: 0x0600213A RID: 8506 RVA: 0x0005A36B File Offset: 0x0005936B
		public IList<long> SourcePosTable { get; set; }

		// Token: 0x17000923 RID: 2339
		// (get) Token: 0x0600213B RID: 8507 RVA: 0x0005A374 File Offset: 0x00059374
		// (set) Token: 0x0600213C RID: 8508 RVA: 0x0005A37C File Offset: 0x0005937C
		public IList<short> LengthTable { get; set; }

		// Token: 0x17000924 RID: 2340
		// (get) Token: 0x0600213D RID: 8509 RVA: 0x0005A385 File Offset: 0x00059385
		// (set) Token: 0x0600213E RID: 8510 RVA: 0x0005A38D File Offset: 0x0005938D
		public IDictionary<int, IList<_ICompilerMessage>> MessageTable { get; set; }
	}
}
