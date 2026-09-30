using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000043 RID: 67
	[ReleasedClass]
	public class LossOfDataWarningEventArgs : EventArgs
	{
		// Token: 0x0600011B RID: 283 RVA: 0x00003E1F File Offset: 0x0000201F
		public LossOfDataWarningEventArgs(int nProjectHandle, Guid objectGuid, Type type)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._type = type;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00003E3C File Offset: 0x0000203C
		public void SetUpgradeProfile()
		{
			this._bUpgradeProfile = true;
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x0600011D RID: 285 RVA: 0x00003E45 File Offset: 0x00002045
		public bool UpgradeProfile
		{
			get
			{
				return this._bUpgradeProfile;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x0600011E RID: 286 RVA: 0x00003E4D File Offset: 0x0000204D
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600011F RID: 287 RVA: 0x00003E55 File Offset: 0x00002055
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000120 RID: 288 RVA: 0x00003E5D File Offset: 0x0000205D
		public Type Type
		{
			get
			{
				return this._type;
			}
		}

		// Token: 0x04000045 RID: 69
		private int _nProjectHandle;

		// Token: 0x04000046 RID: 70
		private Guid _objectGuid;

		// Token: 0x04000047 RID: 71
		private Type _type;

		// Token: 0x04000048 RID: 72
		private bool _bUpgradeProfile;
	}
}
