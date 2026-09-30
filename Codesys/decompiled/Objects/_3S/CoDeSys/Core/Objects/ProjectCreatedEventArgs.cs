using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000075 RID: 117
	[ReleasedClass]
	public class ProjectCreatedEventArgs : EventArgs
	{
		// Token: 0x060001E5 RID: 485 RVA: 0x0000456A File Offset: 0x0000276A
		public ProjectCreatedEventArgs(int nProjectHandle, string stWorkingFolder)
		{
			this._nProjectHandle = nProjectHandle;
			this._stWorkingFolder = stWorkingFolder;
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060001E6 RID: 486 RVA: 0x00004580 File Offset: 0x00002780
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060001E7 RID: 487 RVA: 0x00004588 File Offset: 0x00002788
		public string WorkingFolder
		{
			get
			{
				return this._stWorkingFolder;
			}
		}

		// Token: 0x040000A2 RID: 162
		private int _nProjectHandle;

		// Token: 0x040000A3 RID: 163
		private string _stWorkingFolder;
	}
}
