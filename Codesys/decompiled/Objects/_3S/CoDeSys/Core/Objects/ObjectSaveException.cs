using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C7 RID: 199
	[ReleasedClass]
	public class ObjectSaveException : ObjectManagerException
	{
		// Token: 0x0600032C RID: 812 RVA: 0x00005599 File Offset: 0x00003799
		[Obsolete("Use the other constructor.", true)]
		public ObjectSaveException(int nProjectHandle, Guid objectGuid, string stMessage) : base(nProjectHandle, objectGuid, stMessage)
		{
		}

		// Token: 0x0600032D RID: 813 RVA: 0x000055A4 File Offset: 0x000037A4
		public ObjectSaveException(int nProjectHandle, Guid objectGuid, string stMessage, string stObjectName) : base(nProjectHandle, objectGuid, stMessage, stObjectName)
		{
		}

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x0600032E RID: 814 RVA: 0x000055B1 File Offset: 0x000037B1
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x0600032F RID: 815 RVA: 0x000055B9 File Offset: 0x000037B9
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x06000330 RID: 816 RVA: 0x000055C1 File Offset: 0x000037C1
		public string Reason
		{
			get
			{
				return this._stReason;
			}
		}

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x06000331 RID: 817 RVA: 0x000055C9 File Offset: 0x000037C9
		public override string Message
		{
			get
			{
				return string.Format(Resources.ObjectSaveException, base.ObjectName, this._stReason);
			}
		}
	}
}
