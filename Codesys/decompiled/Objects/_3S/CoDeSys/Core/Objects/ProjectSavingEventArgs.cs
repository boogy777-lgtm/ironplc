using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000085 RID: 133
	[ReleasedClass]
	public class ProjectSavingEventArgs : EventArgs
	{
		// Token: 0x06000222 RID: 546 RVA: 0x00004735 File Offset: 0x00002935
		public ProjectSavingEventArgs(int nProjectHandle, Stream stream, string stStreamName, string stWorkingFolder)
		{
			this._nProjectHandle = nProjectHandle;
			this._stream = stream;
			this._stStreamName = stStreamName;
			this._stWorkingFolder = stWorkingFolder;
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x06000223 RID: 547 RVA: 0x0000475A File Offset: 0x0000295A
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000224 RID: 548 RVA: 0x00004762 File Offset: 0x00002962
		public Stream Stream
		{
			get
			{
				return this._stream;
			}
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x06000225 RID: 549 RVA: 0x0000476A File Offset: 0x0000296A
		public string StreamName
		{
			get
			{
				return this._stStreamName;
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x06000226 RID: 550 RVA: 0x00004772 File Offset: 0x00002972
		public string WorkingFolder
		{
			get
			{
				return this._stWorkingFolder;
			}
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x06000227 RID: 551 RVA: 0x0000477A File Offset: 0x0000297A
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x06000228 RID: 552 RVA: 0x00004782 File Offset: 0x00002982
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

		// Token: 0x040000B8 RID: 184
		private int _nProjectHandle;

		// Token: 0x040000B9 RID: 185
		private Stream _stream;

		// Token: 0x040000BA RID: 186
		private string _stStreamName;

		// Token: 0x040000BB RID: 187
		private string _stWorkingFolder;

		// Token: 0x040000BC RID: 188
		private Exception _ex;
	}
}
