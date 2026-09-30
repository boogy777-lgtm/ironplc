using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000088 RID: 136
	[ReleasedClass]
	public class PromptEncryptionPasswordEventArgs : EventArgs
	{
		// Token: 0x06000230 RID: 560 RVA: 0x000047CE File Offset: 0x000029CE
		public PromptEncryptionPasswordEventArgs(Stream stream, string stStreamName, string stWorkingFolder)
		{
			this._stream = stream;
			this._stStreamName = stStreamName;
			this._stWorkingFolder = stWorkingFolder;
		}

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x06000231 RID: 561 RVA: 0x000047EB File Offset: 0x000029EB
		public Stream Stream
		{
			get
			{
				return this._stream;
			}
		}

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x06000232 RID: 562 RVA: 0x000047F3 File Offset: 0x000029F3
		public string StreamName
		{
			get
			{
				return this._stStreamName;
			}
		}

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x06000233 RID: 563 RVA: 0x000047FB File Offset: 0x000029FB
		public string WorkingFolder
		{
			get
			{
				return this._stWorkingFolder;
			}
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x06000234 RID: 564 RVA: 0x00004803 File Offset: 0x00002A03
		public string Password
		{
			get
			{
				return this._stPassword;
			}
		}

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000235 RID: 565 RVA: 0x0000480B File Offset: 0x00002A0B
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x06000236 RID: 566 RVA: 0x00004813 File Offset: 0x00002A13
		public void Proceed(string stPassword)
		{
			if (stPassword == null)
			{
				throw new ArgumentNullException("stPassword");
			}
			this._stPassword = stPassword;
		}

		// Token: 0x06000237 RID: 567 RVA: 0x0000482A File Offset: 0x00002A2A
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

		// Token: 0x040000BF RID: 191
		private Stream _stream;

		// Token: 0x040000C0 RID: 192
		private string _stStreamName;

		// Token: 0x040000C1 RID: 193
		private string _stWorkingFolder;

		// Token: 0x040000C2 RID: 194
		private Exception _ex;

		// Token: 0x040000C3 RID: 195
		private string _stPassword;
	}
}
