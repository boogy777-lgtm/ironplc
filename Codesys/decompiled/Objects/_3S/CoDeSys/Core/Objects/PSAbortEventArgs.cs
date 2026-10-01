using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200008A RID: 138
	[ReleasedClass]
	public class PSAbortEventArgs : EventArgs
	{
		// Token: 0x0600023C RID: 572 RVA: 0x00004849 File Offset: 0x00002A49
		public PSAbortEventArgs(int nProjectHandle, bool bRollback)
		{
			this._nProjectHandle = nProjectHandle;
			this._bRollback = bRollback;
		}

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x0600023D RID: 573 RVA: 0x0000485F File Offset: 0x00002A5F
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x0600023E RID: 574 RVA: 0x00004867 File Offset: 0x00002A67
		public bool Rollback
		{
			get
			{
				return this._bRollback;
			}
		}

		// Token: 0x040000C4 RID: 196
		private int _nProjectHandle;

		// Token: 0x040000C5 RID: 197
		private bool _bRollback;
	}
}
