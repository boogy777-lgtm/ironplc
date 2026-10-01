using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000062 RID: 98
	[ReleasedClass]
	public class ObjectRenamedEventArgs : EventArgs
	{
		// Token: 0x0600019B RID: 411 RVA: 0x00004342 File Offset: 0x00002542
		public ObjectRenamedEventArgs(int nProjectHandle, Guid objectGuid, string stOldName, string stNewName)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._stOldName = stOldName;
			this._stNewName = stNewName;
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x0600019C RID: 412 RVA: 0x00004367 File Offset: 0x00002567
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x0600019D RID: 413 RVA: 0x0000436F File Offset: 0x0000256F
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x0600019E RID: 414 RVA: 0x00004377 File Offset: 0x00002577
		public string OldName
		{
			get
			{
				return this._stOldName;
			}
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x0600019F RID: 415 RVA: 0x0000437F File Offset: 0x0000257F
		public string NewName
		{
			get
			{
				return this._stNewName;
			}
		}

		// Token: 0x04000088 RID: 136
		private int _nProjectHandle;

		// Token: 0x04000089 RID: 137
		private Guid _objectGuid;

		// Token: 0x0400008A RID: 138
		private string _stOldName;

		// Token: 0x0400008B RID: 139
		private string _stNewName;
	}
}
