using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200005A RID: 90
	[ReleasedClass]
	public class ObjectPropertyEventArgs : EventArgs
	{
		// Token: 0x0600017A RID: 378 RVA: 0x00004253 File Offset: 0x00002453
		public ObjectPropertyEventArgs(int nProjectHandle, Guid objectGuid, IObjectProperty oldProperty, IObjectProperty newProperty)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._oldProperty = oldProperty;
			this._newProperty = newProperty;
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x0600017B RID: 379 RVA: 0x00004278 File Offset: 0x00002478
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x0600017C RID: 380 RVA: 0x00004280 File Offset: 0x00002480
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600017D RID: 381 RVA: 0x00004288 File Offset: 0x00002488
		public IObjectProperty OldProperty
		{
			get
			{
				return this._oldProperty;
			}
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x0600017E RID: 382 RVA: 0x00004290 File Offset: 0x00002490
		public IObjectProperty NewProperty
		{
			get
			{
				return this._newProperty;
			}
		}

		// Token: 0x0400007C RID: 124
		private int _nProjectHandle;

		// Token: 0x0400007D RID: 125
		private Guid _objectGuid;

		// Token: 0x0400007E RID: 126
		private IObjectProperty _oldProperty;

		// Token: 0x0400007F RID: 127
		private IObjectProperty _newProperty;
	}
}
