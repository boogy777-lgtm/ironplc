using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000079 RID: 121
	[ReleasedClass]
	public class ProjectDirtyChangedEventArgs : EventArgs
	{
		// Token: 0x060001F4 RID: 500 RVA: 0x000045CE File Offset: 0x000027CE
		public ProjectDirtyChangedEventArgs(int nProjectHandle)
		{
			this._nProjectHandle = nProjectHandle;
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x060001F5 RID: 501 RVA: 0x000045DD File Offset: 0x000027DD
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x040000A6 RID: 166
		private int _nProjectHandle;
	}
}
