using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000059 RID: 89
	[ReleasedClass]
	public class ObjectPropertyCancelEventArgs : EventArgs
	{
		// Token: 0x06000173 RID: 371 RVA: 0x000041E7 File Offset: 0x000023E7
		public ObjectPropertyCancelEventArgs(int nProjectHandle, Guid objectGuid, IObjectProperty oldProperty, IObjectProperty newProperty)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._oldProperty = oldProperty;
			this._newProperty = newProperty;
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000174 RID: 372 RVA: 0x0000420C File Offset: 0x0000240C
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000175 RID: 373 RVA: 0x00004214 File Offset: 0x00002414
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000176 RID: 374 RVA: 0x0000421C File Offset: 0x0000241C
		public IObjectProperty OldProperty
		{
			get
			{
				return this._oldProperty;
			}
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000177 RID: 375 RVA: 0x00004224 File Offset: 0x00002424
		public IObjectProperty NewProperty
		{
			get
			{
				return this._newProperty;
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000178 RID: 376 RVA: 0x0000422C File Offset: 0x0000242C
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00004234 File Offset: 0x00002434
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

		// Token: 0x04000077 RID: 119
		private int _nProjectHandle;

		// Token: 0x04000078 RID: 120
		private Guid _objectGuid;

		// Token: 0x04000079 RID: 121
		private IObjectProperty _oldProperty;

		// Token: 0x0400007A RID: 122
		private IObjectProperty _newProperty;

		// Token: 0x0400007B RID: 123
		private Exception _ex;
	}
}
