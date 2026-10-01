using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000060 RID: 96
	[ReleasedClass]
	public class ObjectRemovingEventArgs : EventArgs
	{
		// Token: 0x06000190 RID: 400 RVA: 0x000042D6 File Offset: 0x000024D6
		public ObjectRemovingEventArgs(int nRootProjectHandle, Guid rootObjectGuid, int nProjectHandle, Guid objectGuid)
		{
			this._nRootProjectHandle = nRootProjectHandle;
			this._rootObjectGuid = rootObjectGuid;
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000191 RID: 401 RVA: 0x000042FB File Offset: 0x000024FB
		public int RootProjectHandle
		{
			get
			{
				return this._nRootProjectHandle;
			}
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000192 RID: 402 RVA: 0x00004303 File Offset: 0x00002503
		public Guid RootObjectGuid
		{
			get
			{
				return this._rootObjectGuid;
			}
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000193 RID: 403 RVA: 0x0000430B File Offset: 0x0000250B
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000194 RID: 404 RVA: 0x00004313 File Offset: 0x00002513
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000195 RID: 405 RVA: 0x0000431B File Offset: 0x0000251B
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x06000196 RID: 406 RVA: 0x00004323 File Offset: 0x00002523
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

		// Token: 0x04000083 RID: 131
		private int _nRootProjectHandle;

		// Token: 0x04000084 RID: 132
		private Guid _rootObjectGuid;

		// Token: 0x04000085 RID: 133
		private int _nProjectHandle;

		// Token: 0x04000086 RID: 134
		private Guid _objectGuid;

		// Token: 0x04000087 RID: 135
		private Exception _ex;
	}
}
