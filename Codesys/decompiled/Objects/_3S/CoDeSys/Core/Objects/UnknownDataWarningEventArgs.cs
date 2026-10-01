using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000069 RID: 105
	[ReleasedClass]
	public class UnknownDataWarningEventArgs : EventArgs
	{
		// Token: 0x060001BD RID: 445 RVA: 0x00004448 File Offset: 0x00002648
		public UnknownDataWarningEventArgs(int nProjectHandle, Guid objectGuid)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
		}

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x060001BE RID: 446 RVA: 0x0000445E File Offset: 0x0000265E
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x060001BF RID: 447 RVA: 0x00004466 File Offset: 0x00002666
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x04000096 RID: 150
		private int _nProjectHandle;

		// Token: 0x04000097 RID: 151
		private Guid _objectGuid;
	}
}
