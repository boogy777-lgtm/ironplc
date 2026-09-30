using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200005E RID: 94
	[ReleasedClass]
	public class ObjectRemovedEventArgs2 : ObjectRemovedEventArgs
	{
		// Token: 0x06000189 RID: 393 RVA: 0x000042AF File Offset: 0x000024AF
		public ObjectRemovedEventArgs2(int nRootProjectHandle, Guid rootObjectGuid, IMetaObject metaObject) : base(metaObject)
		{
			this._nRootProjectHandle = nRootProjectHandle;
			this._rootObjectGuid = rootObjectGuid;
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x0600018A RID: 394 RVA: 0x000042C6 File Offset: 0x000024C6
		public int RootProjectHandle
		{
			get
			{
				return this._nRootProjectHandle;
			}
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600018B RID: 395 RVA: 0x000042CE File Offset: 0x000024CE
		public Guid RootObjectGuid
		{
			get
			{
				return this._rootObjectGuid;
			}
		}

		// Token: 0x04000081 RID: 129
		private int _nRootProjectHandle;

		// Token: 0x04000082 RID: 130
		private Guid _rootObjectGuid;
	}
}
