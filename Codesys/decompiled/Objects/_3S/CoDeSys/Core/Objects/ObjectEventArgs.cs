using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000051 RID: 81
	[ReleasedClass]
	public class ObjectEventArgs : EventArgs
	{
		// Token: 0x0600014B RID: 331 RVA: 0x0000408C File Offset: 0x0000228C
		public ObjectEventArgs(int nProjectHandle, Guid objectGuid, int nIndex)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._nIndex = nIndex;
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x0600014C RID: 332 RVA: 0x000040A9 File Offset: 0x000022A9
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x0600014D RID: 333 RVA: 0x000040B1 File Offset: 0x000022B1
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600014E RID: 334 RVA: 0x000040B9 File Offset: 0x000022B9
		public int Index
		{
			get
			{
				return this._nIndex;
			}
		}

		// Token: 0x04000064 RID: 100
		private int _nProjectHandle;

		// Token: 0x04000065 RID: 101
		private Guid _objectGuid;

		// Token: 0x04000066 RID: 102
		private int _nIndex;
	}
}
