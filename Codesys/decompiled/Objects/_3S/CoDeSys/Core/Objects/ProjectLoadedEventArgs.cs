using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200007C RID: 124
	[ReleasedClass]
	public class ProjectLoadedEventArgs : EventArgs
	{
		// Token: 0x060001FD RID: 509 RVA: 0x0000460B File Offset: 0x0000280B
		public ProjectLoadedEventArgs(int nProjectHandle, Stream stream, string stStreamName, string stWorkingFolder)
		{
			this._nProjectHandle = nProjectHandle;
			this._stream = stream;
			this._stStreamName = stStreamName;
			this._stWorkingFolder = stWorkingFolder;
		}

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x060001FE RID: 510 RVA: 0x00004630 File Offset: 0x00002830
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060001FF RID: 511 RVA: 0x00004638 File Offset: 0x00002838
		public Stream Stream
		{
			get
			{
				return this._stream;
			}
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x06000200 RID: 512 RVA: 0x00004640 File Offset: 0x00002840
		public string StreamName
		{
			get
			{
				return this._stStreamName;
			}
		}

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x06000201 RID: 513 RVA: 0x00004648 File Offset: 0x00002848
		public string WorkingFolder
		{
			get
			{
				return this._stWorkingFolder;
			}
		}

		// Token: 0x040000A9 RID: 169
		private int _nProjectHandle;

		// Token: 0x040000AA RID: 170
		private Stream _stream;

		// Token: 0x040000AB RID: 171
		private string _stStreamName;

		// Token: 0x040000AC RID: 172
		private string _stWorkingFolder;
	}
}
