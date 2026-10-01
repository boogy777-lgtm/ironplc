using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000CB RID: 203
	[ReleasedClass]
	public class ProjectSaveException : ObjectManagerException
	{
		// Token: 0x0600033B RID: 827 RVA: 0x0000568F File Offset: 0x0000388F
		public ProjectSaveException(int nProjectHandle, string stMessage) : base(nProjectHandle, Guid.Empty, stMessage, string.Empty)
		{
		}

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x0600033C RID: 828 RVA: 0x000056A3 File Offset: 0x000038A3
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x0600033D RID: 829 RVA: 0x000056AB File Offset: 0x000038AB
		public string Reason
		{
			get
			{
				return this._stReason;
			}
		}

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x0600033E RID: 830 RVA: 0x000056B3 File Offset: 0x000038B3
		public override string Message
		{
			get
			{
				return string.Format(Resources.ProjectSaveException, this._stReason);
			}
		}
	}
}
