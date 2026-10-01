using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000082 RID: 130
	[ReleasedClass]
	public class ProjectSavedEventArgs : EventArgs
	{
		// Token: 0x06000216 RID: 534 RVA: 0x000046C3 File Offset: 0x000028C3
		public ProjectSavedEventArgs(int nProjectHandle, Stream stream, string stStreamName, string stWorkingFolder)
		{
			this._nProjectHandle = nProjectHandle;
			this._stream = stream;
			this._stStreamName = stStreamName;
			this._stWorkingFolder = stWorkingFolder;
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000217 RID: 535 RVA: 0x000046E8 File Offset: 0x000028E8
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000218 RID: 536 RVA: 0x000046F0 File Offset: 0x000028F0
		public Stream Stream
		{
			get
			{
				return this._stream;
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000219 RID: 537 RVA: 0x000046F8 File Offset: 0x000028F8
		public string StreamName
		{
			get
			{
				return this._stStreamName;
			}
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x0600021A RID: 538 RVA: 0x00004700 File Offset: 0x00002900
		public string WorkingFolder
		{
			get
			{
				return this._stWorkingFolder;
			}
		}

		// Token: 0x040000B2 RID: 178
		private int _nProjectHandle;

		// Token: 0x040000B3 RID: 179
		private Stream _stream;

		// Token: 0x040000B4 RID: 180
		private string _stStreamName;

		// Token: 0x040000B5 RID: 181
		private string _stWorkingFolder;
	}
}
