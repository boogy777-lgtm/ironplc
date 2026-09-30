using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000083 RID: 131
	[ReleasedClass]
	public class ProjectSavedEventArgs2 : ProjectSavedEventArgs
	{
		// Token: 0x0600021B RID: 539 RVA: 0x00004708 File Offset: 0x00002908
		public ProjectSavedEventArgs2(int nProjectHandle, Stream stream, string stStreamName, string stWorkingFolder, Profile profile, string stProfileName) : base(nProjectHandle, stream, stStreamName, stWorkingFolder)
		{
			this._profile = profile;
			this._stProfileName = stProfileName;
		}

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x0600021C RID: 540 RVA: 0x00004725 File Offset: 0x00002925
		public Profile Profile
		{
			get
			{
				return this._profile;
			}
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x0600021D RID: 541 RVA: 0x0000472D File Offset: 0x0000292D
		public string ProfileName
		{
			get
			{
				return this._stProfileName;
			}
		}

		// Token: 0x040000B6 RID: 182
		private Profile _profile;

		// Token: 0x040000B7 RID: 183
		private string _stProfileName;
	}
}
