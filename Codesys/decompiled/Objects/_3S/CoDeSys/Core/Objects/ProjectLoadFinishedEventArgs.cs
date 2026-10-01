using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200007E RID: 126
	[ReleasedClass]
	public class ProjectLoadFinishedEventArgs : EventArgs
	{
		// Token: 0x06000206 RID: 518 RVA: 0x00004650 File Offset: 0x00002850
		public ProjectLoadFinishedEventArgs(int nProjectHandle)
		{
			this._nProjectHandle = nProjectHandle;
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000207 RID: 519 RVA: 0x0000465F File Offset: 0x0000285F
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x040000AD RID: 173
		private int _nProjectHandle;
	}
}
