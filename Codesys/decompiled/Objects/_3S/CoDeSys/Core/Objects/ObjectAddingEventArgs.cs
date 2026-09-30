using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200004A RID: 74
	[ReleasedClass]
	public class ObjectAddingEventArgs : EventArgs
	{
		// Token: 0x0600012D RID: 301 RVA: 0x00003F39 File Offset: 0x00002139
		public ObjectAddingEventArgs(int nProjectHandle, Guid parentObjectGuid, IObject obj, string stName, int nIndex)
		{
			this._nProjectHandle = nProjectHandle;
			this._parentObjectGuid = parentObjectGuid;
			this._obj = obj;
			this._stName = stName;
			this._nIndex = nIndex;
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x0600012E RID: 302 RVA: 0x00003F66 File Offset: 0x00002166
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x0600012F RID: 303 RVA: 0x00003F6E File Offset: 0x0000216E
		public Guid ParentObjectGuid
		{
			get
			{
				return this._parentObjectGuid;
			}
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000130 RID: 304 RVA: 0x00003F76 File Offset: 0x00002176
		public IObject Object
		{
			get
			{
				return this._obj;
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000131 RID: 305 RVA: 0x00003F7E File Offset: 0x0000217E
		public string Name
		{
			get
			{
				return this._stName;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000132 RID: 306 RVA: 0x00003F86 File Offset: 0x00002186
		public int Index
		{
			get
			{
				return this._nIndex;
			}
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000133 RID: 307 RVA: 0x00003F8E File Offset: 0x0000218E
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00003F96 File Offset: 0x00002196
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

		// Token: 0x04000057 RID: 87
		private int _nProjectHandle;

		// Token: 0x04000058 RID: 88
		private Guid _parentObjectGuid;

		// Token: 0x04000059 RID: 89
		private IObject _obj;

		// Token: 0x0400005A RID: 90
		private string _stName;

		// Token: 0x0400005B RID: 91
		private int _nIndex;

		// Token: 0x0400005C RID: 92
		private Exception _ex;
	}
}
