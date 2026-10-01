using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000057 RID: 87
	[ReleasedClass]
	public class ObjectMovingEventArgs : EventArgs
	{
		// Token: 0x06000166 RID: 358 RVA: 0x0000415B File Offset: 0x0000235B
		public ObjectMovingEventArgs(int nProjectHandle, Guid objectGuid, Guid oldParentObjectGuid, Guid newParentObjectGuid, int nOldIndex, int nNewIndex)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._oldParentObjectGuid = oldParentObjectGuid;
			this._newParentObjectGuid = newParentObjectGuid;
			this._nOldIndex = nOldIndex;
			this._nNewIndex = nNewIndex;
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000167 RID: 359 RVA: 0x00004190 File Offset: 0x00002390
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000168 RID: 360 RVA: 0x00004198 File Offset: 0x00002398
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000169 RID: 361 RVA: 0x000041A0 File Offset: 0x000023A0
		public Guid OldParentObjectGuid
		{
			get
			{
				return this._oldParentObjectGuid;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x0600016A RID: 362 RVA: 0x000041A8 File Offset: 0x000023A8
		public Guid NewParentObjectGuid
		{
			get
			{
				return this._newParentObjectGuid;
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600016B RID: 363 RVA: 0x000041B0 File Offset: 0x000023B0
		public int OldIndex
		{
			get
			{
				return this._nOldIndex;
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600016C RID: 364 RVA: 0x000041B8 File Offset: 0x000023B8
		public int NewIndex
		{
			get
			{
				return this._nNewIndex;
			}
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x0600016D RID: 365 RVA: 0x000041C0 File Offset: 0x000023C0
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x0600016E RID: 366 RVA: 0x000041C8 File Offset: 0x000023C8
		public void Cancel(Exception ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullException("ex");
			}
			if (this._ex == null)
			{
				this._ex = ex;
			}
		}

		// Token: 0x04000070 RID: 112
		private int _nProjectHandle;

		// Token: 0x04000071 RID: 113
		private Guid _objectGuid;

		// Token: 0x04000072 RID: 114
		private Guid _oldParentObjectGuid;

		// Token: 0x04000073 RID: 115
		private Guid _newParentObjectGuid;

		// Token: 0x04000074 RID: 116
		private int _nOldIndex;

		// Token: 0x04000075 RID: 117
		private int _nNewIndex;

		// Token: 0x04000076 RID: 118
		private Exception _ex;
	}
}
