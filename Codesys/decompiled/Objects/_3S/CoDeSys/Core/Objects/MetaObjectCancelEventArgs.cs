using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000045 RID: 69
	[ReleasedClass]
	public class MetaObjectCancelEventArgs : ObjectCancelEventArgs
	{
		// Token: 0x06000124 RID: 292 RVA: 0x00003E81 File Offset: 0x00002081
		public MetaObjectCancelEventArgs(IMetaObject mo) : base(mo.ProjectHandle, mo.ObjectGuid, mo.Index)
		{
			this._mo = mo;
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000125 RID: 293 RVA: 0x00003EA2 File Offset: 0x000020A2
		public IMetaObject MetaObject
		{
			get
			{
				return this._mo;
			}
		}

		// Token: 0x0400004A RID: 74
		private readonly IMetaObject _mo;
	}
}
