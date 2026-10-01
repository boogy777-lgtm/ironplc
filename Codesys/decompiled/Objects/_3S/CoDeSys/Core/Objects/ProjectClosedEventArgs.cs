using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000071 RID: 113
	[ReleasedClass]
	public class ProjectClosedEventArgs : EventArgs
	{
		// Token: 0x060001D7 RID: 471 RVA: 0x00004515 File Offset: 0x00002715
		public ProjectClosedEventArgs(int nProjectHandle)
		{
			this._nProjectHandle = nProjectHandle;
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x060001D8 RID: 472 RVA: 0x00004524 File Offset: 0x00002724
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x0400009F RID: 159
		private int _nProjectHandle;
	}
}
