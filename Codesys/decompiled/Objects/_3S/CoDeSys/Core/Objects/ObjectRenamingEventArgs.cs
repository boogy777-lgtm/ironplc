using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000064 RID: 100
	[ReleasedClass]
	public class ObjectRenamingEventArgs : EventArgs
	{
		// Token: 0x060001A4 RID: 420 RVA: 0x00004387 File Offset: 0x00002587
		public ObjectRenamingEventArgs(int nProjectHandle, Guid objectGuid, string stOldName, string stNewName)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._stOldName = stOldName;
			this._stNewName = stNewName;
		}

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x060001A5 RID: 421 RVA: 0x000043AC File Offset: 0x000025AC
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x060001A6 RID: 422 RVA: 0x000043B4 File Offset: 0x000025B4
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x060001A7 RID: 423 RVA: 0x000043BC File Offset: 0x000025BC
		public string OldName
		{
			get
			{
				return this._stOldName;
			}
		}

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x060001A8 RID: 424 RVA: 0x000043C4 File Offset: 0x000025C4
		public string NewName
		{
			get
			{
				return this._stNewName;
			}
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x000043CC File Offset: 0x000025CC
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x060001AA RID: 426 RVA: 0x000043D4 File Offset: 0x000025D4
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

		// Token: 0x0400008C RID: 140
		private int _nProjectHandle;

		// Token: 0x0400008D RID: 141
		private Guid _objectGuid;

		// Token: 0x0400008E RID: 142
		private string _stOldName;

		// Token: 0x0400008F RID: 143
		private string _stNewName;

		// Token: 0x04000090 RID: 144
		private Exception _ex;
	}
}
