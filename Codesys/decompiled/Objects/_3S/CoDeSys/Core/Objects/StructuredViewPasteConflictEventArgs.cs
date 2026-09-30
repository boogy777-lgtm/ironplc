using System;
using System.ComponentModel;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200009B RID: 155
	[ReleasedClass]
	public class StructuredViewPasteConflictEventArgs : CancelEventArgs
	{
		// Token: 0x06000279 RID: 633 RVA: 0x00004C7E File Offset: 0x00002E7E
		public StructuredViewPasteConflictEventArgs(PasteConflict[] conflicts)
		{
			if (conflicts == null)
			{
				throw new ArgumentNullException("conflicts");
			}
			this._conflicts = conflicts;
			this._overwriteFlags = new bool[this._conflicts.Length];
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x0600027A RID: 634 RVA: 0x00004CAE File Offset: 0x00002EAE
		public PasteConflict[] Conflicts
		{
			get
			{
				return this._conflicts;
			}
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00004CB6 File Offset: 0x00002EB6
		public void SetOverwriteFlag(int nConflictIndex, bool bOverwrite)
		{
			this._overwriteFlags[nConflictIndex] = bOverwrite;
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00004CC1 File Offset: 0x00002EC1
		public bool GetOverwriteFlag(int nConflictIndex)
		{
			return this._overwriteFlags[nConflictIndex];
		}

		// Token: 0x040000F1 RID: 241
		private PasteConflict[] _conflicts;

		// Token: 0x040000F2 RID: 242
		private bool[] _overwriteFlags;
	}
}
