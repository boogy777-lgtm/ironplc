using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000BF RID: 191
	[ReleasedClass]
	public class ObjectLoadException : ObjectManagerException
	{
		// Token: 0x0600030A RID: 778 RVA: 0x00005391 File Offset: 0x00003591
		[Obsolete("Use the other constructor.", true)]
		public ObjectLoadException(int nProjectHandle, Guid objectGuid, string stMessage) : base(nProjectHandle, objectGuid, stMessage)
		{
		}

		// Token: 0x0600030B RID: 779 RVA: 0x0000539C File Offset: 0x0000359C
		public ObjectLoadException(int nProjectHandle, Guid objectGuid, string stMessage, string stObjectName) : base(nProjectHandle, objectGuid, stMessage, stObjectName)
		{
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x0600030C RID: 780 RVA: 0x000053A9 File Offset: 0x000035A9
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x0600030D RID: 781 RVA: 0x000053B1 File Offset: 0x000035B1
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x0600030E RID: 782 RVA: 0x000053B9 File Offset: 0x000035B9
		public string Reason
		{
			get
			{
				return this._stReason;
			}
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x0600030F RID: 783 RVA: 0x000053C1 File Offset: 0x000035C1
		public override string Message
		{
			get
			{
				return string.Format(Resources.ObjectLoadException, base.ObjectName, this._stReason);
			}
		}
	}
}
