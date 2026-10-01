using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000AD RID: 173
	[ReleasedClass]
	public class SVNodesCopyingEventArgs : EventArgs
	{
		// Token: 0x060002CA RID: 714 RVA: 0x00004F64 File Offset: 0x00003164
		public SVNodesCopyingEventArgs(int projectHandle, IList<Guid> objectsToCopy)
		{
			this._projectHandle = projectHandle;
			this._objectsToCopy = objectsToCopy;
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x060002CB RID: 715 RVA: 0x00004F7A File Offset: 0x0000317A
		public int ProjectHandle
		{
			get
			{
				return this._projectHandle;
			}
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x060002CC RID: 716 RVA: 0x00004F82 File Offset: 0x00003182
		public IList<Guid> ObjectsToCopy
		{
			get
			{
				return this._objectsToCopy;
			}
		}

		// Token: 0x04000113 RID: 275
		private int _projectHandle;

		// Token: 0x04000114 RID: 276
		private IList<Guid> _objectsToCopy;
	}
}
