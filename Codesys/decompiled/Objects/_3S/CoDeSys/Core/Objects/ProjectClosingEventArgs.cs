using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000073 RID: 115
	[ReleasedClass]
	public class ProjectClosingEventArgs : EventArgs
	{
		// Token: 0x060001DD RID: 477 RVA: 0x0000452C File Offset: 0x0000272C
		public ProjectClosingEventArgs(int nProjectHandle)
		{
			this._nProjectHandle = nProjectHandle;
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060001DE RID: 478 RVA: 0x0000453B File Offset: 0x0000273B
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060001DF RID: 479 RVA: 0x00004543 File Offset: 0x00002743
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000454B File Offset: 0x0000274B
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

		// Token: 0x040000A0 RID: 160
		private int _nProjectHandle;

		// Token: 0x040000A1 RID: 161
		private Exception _ex;
	}
}
