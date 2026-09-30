using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000053 RID: 83
	[ReleasedClass]
	public class ObjectModifiedEventArgs : EventArgs
	{
		// Token: 0x06000153 RID: 339 RVA: 0x000040C1 File Offset: 0x000022C1
		public ObjectModifiedEventArgs(int nProjectHandle, Guid objectGuid, object editor)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._editor = editor;
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000154 RID: 340 RVA: 0x000040DE File Offset: 0x000022DE
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000155 RID: 341 RVA: 0x000040E6 File Offset: 0x000022E6
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000156 RID: 342 RVA: 0x000040EE File Offset: 0x000022EE
		public object Editor
		{
			get
			{
				return this._editor;
			}
		}

		// Token: 0x04000067 RID: 103
		private int _nProjectHandle;

		// Token: 0x04000068 RID: 104
		private Guid _objectGuid;

		// Token: 0x04000069 RID: 105
		private object _editor;
	}
}
