using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200004F RID: 79
	[ReleasedClass]
	public class ObjectCancelEventArgs : EventArgs
	{
		// Token: 0x06000141 RID: 321 RVA: 0x00004030 File Offset: 0x00002230
		public ObjectCancelEventArgs(int nProjectHandle, Guid objectGuid, int nIndex)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._nIndex = nIndex;
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x06000142 RID: 322 RVA: 0x0000404D File Offset: 0x0000224D
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x06000143 RID: 323 RVA: 0x00004055 File Offset: 0x00002255
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000144 RID: 324 RVA: 0x0000405D File Offset: 0x0000225D
		public int Index
		{
			get
			{
				return this._nIndex;
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000145 RID: 325 RVA: 0x00004065 File Offset: 0x00002265
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x06000146 RID: 326 RVA: 0x0000406D File Offset: 0x0000226D
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

		// Token: 0x04000060 RID: 96
		private int _nProjectHandle;

		// Token: 0x04000061 RID: 97
		private Guid _objectGuid;

		// Token: 0x04000062 RID: 98
		private int _nIndex;

		// Token: 0x04000063 RID: 99
		private Exception _ex;
	}
}
