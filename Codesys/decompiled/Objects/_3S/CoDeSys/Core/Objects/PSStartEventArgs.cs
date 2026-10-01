using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000096 RID: 150
	[ReleasedClass]
	public class PSStartEventArgs : EventArgs
	{
		// Token: 0x06000266 RID: 614 RVA: 0x00004B97 File Offset: 0x00002D97
		public PSStartEventArgs(int nProjectHandle)
		{
			this._nProjectHandle = nProjectHandle;
		}

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x06000267 RID: 615 RVA: 0x00004BA6 File Offset: 0x00002DA6
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x040000E7 RID: 231
		private int _nProjectHandle;
	}
}
