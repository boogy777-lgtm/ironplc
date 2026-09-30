using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200008C RID: 140
	[ReleasedClass]
	public class PSChangedEventArgs2 : PSChangedEventArgs
	{
		// Token: 0x06000245 RID: 581 RVA: 0x000049D8 File Offset: 0x00002BD8
		public PSChangedEventArgs2(int nProjectHandle, IPSChange[] changes) : base(changes)
		{
			this._nProjectHandle = nProjectHandle;
		}

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x06000246 RID: 582 RVA: 0x000049E8 File Offset: 0x00002BE8
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x040000C7 RID: 199
		private int _nProjectHandle;
	}
}
