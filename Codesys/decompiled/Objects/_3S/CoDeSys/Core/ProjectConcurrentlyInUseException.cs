using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000019 RID: 25
	[ReleasedClass]
	public class ProjectConcurrentlyInUseException : ProjectLoadException
	{
		// Token: 0x06000069 RID: 105 RVA: 0x0000270D File Offset: 0x0000090D
		public ProjectConcurrentlyInUseException(string message, string projectPath, string userName, string machineName, bool handled) : base(message)
		{
			this._message = message;
			this._projectPath = projectPath;
			this._userName = userName;
			this._machineName = machineName;
			this._handled = handled;
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x0600006A RID: 106 RVA: 0x0000273B File Offset: 0x0000093B
		public string ProjectPath
		{
			get
			{
				return this._projectPath;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600006B RID: 107 RVA: 0x00002743 File Offset: 0x00000943
		public string UserName
		{
			get
			{
				return this._userName;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600006C RID: 108 RVA: 0x0000274B File Offset: 0x0000094B
		public string MachineName
		{
			get
			{
				return this._machineName;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00002753 File Offset: 0x00000953
		// (set) Token: 0x0600006E RID: 110 RVA: 0x0000275B File Offset: 0x0000095B
		public bool Handled
		{
			get
			{
				return this._handled;
			}
			set
			{
				this._handled = value;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00002764 File Offset: 0x00000964
		public override string Message
		{
			get
			{
				return this._message;
			}
		}

		// Token: 0x04000016 RID: 22
		private string _message;

		// Token: 0x04000017 RID: 23
		private string _projectPath;

		// Token: 0x04000018 RID: 24
		private string _userName;

		// Token: 0x04000019 RID: 25
		private string _machineName;

		// Token: 0x0400001A RID: 26
		private bool _handled;
	}
}
