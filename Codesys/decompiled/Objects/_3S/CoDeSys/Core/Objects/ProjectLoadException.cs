using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000CA RID: 202
	[ReleasedClass]
	public class ProjectLoadException : ObjectManagerException
	{
		// Token: 0x06000338 RID: 824 RVA: 0x00005661 File Offset: 0x00003861
		public ProjectLoadException(string stMessage) : base(-1, Guid.Empty, stMessage, string.Empty)
		{
		}

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000339 RID: 825 RVA: 0x00005675 File Offset: 0x00003875
		public string Reason
		{
			get
			{
				return this._stReason;
			}
		}

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x0600033A RID: 826 RVA: 0x0000567D File Offset: 0x0000387D
		public override string Message
		{
			get
			{
				return string.Format(Resources.ProjectLoadException, this._stReason);
			}
		}
	}
}
