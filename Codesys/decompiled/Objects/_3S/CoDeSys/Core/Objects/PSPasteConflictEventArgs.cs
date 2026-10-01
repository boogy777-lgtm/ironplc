using System;
using System.ComponentModel;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000092 RID: 146
	[ReleasedClass]
	public class PSPasteConflictEventArgs : CancelEventArgs
	{
		// Token: 0x0600024D RID: 589 RVA: 0x00004A37 File Offset: 0x00002C37
		public PSPasteConflictEventArgs(PSPasteConflict[] conflicts)
		{
			if (conflicts == null)
			{
				throw new ArgumentNullException("conflicts");
			}
			this._conflicts = conflicts;
			this._overwriteFlags = new bool[this._conflicts.Length];
		}

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x0600024E RID: 590 RVA: 0x00004A67 File Offset: 0x00002C67
		public PSPasteConflict[] Conflicts
		{
			get
			{
				return this._conflicts;
			}
		}

		// Token: 0x0600024F RID: 591 RVA: 0x00004A6F File Offset: 0x00002C6F
		public void SetOverwriteFlag(int conflictIndex, bool overwrite)
		{
			this._overwriteFlags[conflictIndex] = overwrite;
		}

		// Token: 0x06000250 RID: 592 RVA: 0x00004A7A File Offset: 0x00002C7A
		public bool GetOverwriteFlag(int conflictIndex)
		{
			return this._overwriteFlags[conflictIndex];
		}

		// Token: 0x040000D5 RID: 213
		private PSPasteConflict[] _conflicts;

		// Token: 0x040000D6 RID: 214
		private bool[] _overwriteFlags;
	}
}
