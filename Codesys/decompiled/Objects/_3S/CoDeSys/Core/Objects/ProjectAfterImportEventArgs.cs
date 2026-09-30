using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200006D RID: 109
	[ReleasedClass]
	public class ProjectAfterImportEventArgs : ProjectImportEventArgs
	{
		// Token: 0x060001CC RID: 460 RVA: 0x000044F2 File Offset: 0x000026F2
		public ProjectAfterImportEventArgs(IProjectConverter converter, int nProjectHandle, Exception ex) : base(converter, nProjectHandle)
		{
			this._exProblem = ex;
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x060001CD RID: 461 RVA: 0x00004503 File Offset: 0x00002703
		public Exception Exception
		{
			get
			{
				return this._exProblem;
			}
		}

		// Token: 0x0400009E RID: 158
		private Exception _exProblem;
	}
}
