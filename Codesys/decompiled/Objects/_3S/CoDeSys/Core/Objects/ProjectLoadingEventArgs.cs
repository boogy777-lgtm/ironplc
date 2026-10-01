using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000080 RID: 128
	[ReleasedClass]
	public class ProjectLoadingEventArgs : EventArgs
	{
		// Token: 0x0600020C RID: 524 RVA: 0x00004667 File Offset: 0x00002867
		public ProjectLoadingEventArgs(Stream stream, string stStreamName, string stWorkingFolder)
		{
			this._stream = stream;
			this._stStreamName = stStreamName;
			this._stWorkingFolder = stWorkingFolder;
		}

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x0600020D RID: 525 RVA: 0x00004684 File Offset: 0x00002884
		public Stream Stream
		{
			get
			{
				return this._stream;
			}
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x0600020E RID: 526 RVA: 0x0000468C File Offset: 0x0000288C
		public string StreamName
		{
			get
			{
				return this._stStreamName;
			}
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x0600020F RID: 527 RVA: 0x00004694 File Offset: 0x00002894
		public string WorkingFolder
		{
			get
			{
				return this._stWorkingFolder;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000210 RID: 528 RVA: 0x0000469C File Offset: 0x0000289C
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x06000211 RID: 529 RVA: 0x000046A4 File Offset: 0x000028A4
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

		// Token: 0x040000AE RID: 174
		private Stream _stream;

		// Token: 0x040000AF RID: 175
		private string _stStreamName;

		// Token: 0x040000B0 RID: 176
		private string _stWorkingFolder;

		// Token: 0x040000B1 RID: 177
		private Exception _ex;
	}
}
