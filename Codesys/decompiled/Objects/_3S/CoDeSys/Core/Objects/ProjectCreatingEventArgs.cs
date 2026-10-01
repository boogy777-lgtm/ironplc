using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000077 RID: 119
	[ReleasedClass]
	public class ProjectCreatingEventArgs : EventArgs
	{
		// Token: 0x060001EC RID: 492 RVA: 0x00004590 File Offset: 0x00002790
		public ProjectCreatingEventArgs(string stWorkingFolder)
		{
			this._stWorkingFolder = stWorkingFolder;
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060001ED RID: 493 RVA: 0x0000459F File Offset: 0x0000279F
		public string WorkingFolder
		{
			get
			{
				return this._stWorkingFolder;
			}
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060001EE RID: 494 RVA: 0x000045A7 File Offset: 0x000027A7
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x060001EF RID: 495 RVA: 0x000045AF File Offset: 0x000027AF
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

		// Token: 0x040000A4 RID: 164
		private string _stWorkingFolder;

		// Token: 0x040000A5 RID: 165
		private Exception _ex;
	}
}
