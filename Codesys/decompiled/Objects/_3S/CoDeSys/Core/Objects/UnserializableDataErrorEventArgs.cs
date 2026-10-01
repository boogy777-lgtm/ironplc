using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200006B RID: 107
	[ReleasedClass]
	public class UnserializableDataErrorEventArgs : EventArgs
	{
		// Token: 0x060001C3 RID: 451 RVA: 0x00004490 File Offset: 0x00002690
		public UnserializableDataErrorEventArgs(int nProjectHandle, Guid objectGuid, Type type)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._type = type;
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x000044AD File Offset: 0x000026AD
		public void SetUpgradeProfile()
		{
			this._bUpgradeProfile = true;
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x060001C5 RID: 453 RVA: 0x000044B6 File Offset: 0x000026B6
		public bool UpgradeProfile
		{
			get
			{
				return this._bUpgradeProfile;
			}
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060001C6 RID: 454 RVA: 0x000044BE File Offset: 0x000026BE
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060001C7 RID: 455 RVA: 0x000044C6 File Offset: 0x000026C6
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x060001C8 RID: 456 RVA: 0x000044CE File Offset: 0x000026CE
		public Type Type
		{
			get
			{
				return this._type;
			}
		}

		// Token: 0x04000099 RID: 153
		private int _nProjectHandle;

		// Token: 0x0400009A RID: 154
		private Guid _objectGuid;

		// Token: 0x0400009B RID: 155
		private Type _type;

		// Token: 0x0400009C RID: 156
		private bool _bUpgradeProfile;
	}
}
