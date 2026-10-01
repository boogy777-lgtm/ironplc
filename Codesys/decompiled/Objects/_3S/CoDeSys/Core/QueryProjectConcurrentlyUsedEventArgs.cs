using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000018 RID: 24
	[ReleasedClass]
	public class QueryProjectConcurrentlyUsedEventArgs : EventArgs
	{
		// Token: 0x06000062 RID: 98 RVA: 0x000026A8 File Offset: 0x000008A8
		public QueryProjectConcurrentlyUsedEventArgs(string projectPath, string userName, string machineName)
		{
			this._projectPath = projectPath;
			this._userName = userName;
			this._machineName = machineName;
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000063 RID: 99 RVA: 0x000026C5 File Offset: 0x000008C5
		public string ProjectPath
		{
			get
			{
				return this._projectPath;
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000064 RID: 100 RVA: 0x000026CD File Offset: 0x000008CD
		public string UserName
		{
			get
			{
				return this._userName;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000065 RID: 101 RVA: 0x000026D5 File Offset: 0x000008D5
		public string MachineName
		{
			get
			{
				return this._machineName;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000066 RID: 102 RVA: 0x000026DD File Offset: 0x000008DD
		public bool Open
		{
			get
			{
				return this._open;
			}
		}

		// Token: 0x06000067 RID: 103 RVA: 0x000026E5 File Offset: 0x000008E5
		public void CancelOpen()
		{
			this._handled = true;
			this._open = false;
		}

		// Token: 0x06000068 RID: 104 RVA: 0x000026F5 File Offset: 0x000008F5
		public void ContinueOpen()
		{
			if (!this._handled)
			{
				this._open = true;
				this._handled = true;
			}
		}

		// Token: 0x04000011 RID: 17
		private string _projectPath;

		// Token: 0x04000012 RID: 18
		private string _userName;

		// Token: 0x04000013 RID: 19
		private string _machineName;

		// Token: 0x04000014 RID: 20
		private bool _handled;

		// Token: 0x04000015 RID: 21
		private bool _open;
	}
}
