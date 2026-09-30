using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200005D RID: 93
	[ReleasedClass]
	public class ObjectRemovedEventArgs : EventArgs
	{
		// Token: 0x06000187 RID: 391 RVA: 0x00004298 File Offset: 0x00002498
		public ObjectRemovedEventArgs(IMetaObject metaObject)
		{
			this._metaObject = metaObject;
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000188 RID: 392 RVA: 0x000042A7 File Offset: 0x000024A7
		public IMetaObject MetaObject
		{
			get
			{
				return this._metaObject;
			}
		}

		// Token: 0x04000080 RID: 128
		private IMetaObject _metaObject;
	}
}
