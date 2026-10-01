using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000095 RID: 149
	[ReleasedClass]
	public class PSQueryRollbackEventArgs : EventArgs
	{
		// Token: 0x06000262 RID: 610 RVA: 0x00004B6F File Offset: 0x00002D6F
		public PSQueryRollbackEventArgs(PSChangedEventArgs2 changedEventArgs)
		{
			this._changedEventArgs = changedEventArgs;
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x06000263 RID: 611 RVA: 0x00004B7E File Offset: 0x00002D7E
		public PSChangedEventArgs2 ChangedEventArgs
		{
			get
			{
				return this._changedEventArgs;
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000264 RID: 612 RVA: 0x00004B86 File Offset: 0x00002D86
		public bool Rollback
		{
			get
			{
				return this._bRollback;
			}
		}

		// Token: 0x06000265 RID: 613 RVA: 0x00004B8E File Offset: 0x00002D8E
		public void DoRollback()
		{
			this._bRollback = true;
		}

		// Token: 0x040000E5 RID: 229
		private PSChangedEventArgs2 _changedEventArgs;

		// Token: 0x040000E6 RID: 230
		private bool _bRollback;
	}
}
