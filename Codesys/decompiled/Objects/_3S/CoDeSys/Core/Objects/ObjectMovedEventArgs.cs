using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000055 RID: 85
	[ReleasedClass]
	public class ObjectMovedEventArgs : EventArgs
	{
		// Token: 0x0600015B RID: 347 RVA: 0x000040F6 File Offset: 0x000022F6
		public ObjectMovedEventArgs(int nProjectHandle, Guid objectGuid, Guid oldParentObjectGuid, Guid newParentObjectGuid, int nOldIndex, int nNewIndex)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._oldParentObjectGuid = oldParentObjectGuid;
			this._newParentObjectGuid = newParentObjectGuid;
			this._nOldIndex = nOldIndex;
			this._nNewIndex = nNewIndex;
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x0600015C RID: 348 RVA: 0x0000412B File Offset: 0x0000232B
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x0600015D RID: 349 RVA: 0x00004133 File Offset: 0x00002333
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x0600015E RID: 350 RVA: 0x0000413B File Offset: 0x0000233B
		public Guid OldParentObjectGuid
		{
			get
			{
				return this._oldParentObjectGuid;
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x0600015F RID: 351 RVA: 0x00004143 File Offset: 0x00002343
		public Guid NewParentObjectGuid
		{
			get
			{
				return this._newParentObjectGuid;
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000160 RID: 352 RVA: 0x0000414B File Offset: 0x0000234B
		public int OldIndex
		{
			get
			{
				return this._nOldIndex;
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000161 RID: 353 RVA: 0x00004153 File Offset: 0x00002353
		public int NewIndex
		{
			get
			{
				return this._nNewIndex;
			}
		}

		// Token: 0x0400006A RID: 106
		private int _nProjectHandle;

		// Token: 0x0400006B RID: 107
		private Guid _objectGuid;

		// Token: 0x0400006C RID: 108
		private Guid _oldParentObjectGuid;

		// Token: 0x0400006D RID: 109
		private Guid _newParentObjectGuid;

		// Token: 0x0400006E RID: 110
		private int _nOldIndex;

		// Token: 0x0400006F RID: 111
		private int _nNewIndex;
	}
}
