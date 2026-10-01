using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000086 RID: 134
	[ReleasedClass]
	public class ProjectSavingEventArgs2 : ProjectSavingEventArgs
	{
		// Token: 0x06000229 RID: 553 RVA: 0x000047A1 File Offset: 0x000029A1
		public ProjectSavingEventArgs2(int nProjectHandle, Stream stream, string stStreamName, string stWorkingFolder, Profile profile, string stProfileName) : base(nProjectHandle, stream, stStreamName, stWorkingFolder)
		{
			this._profile = profile;
			this._stProfileName = stProfileName;
		}

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x0600022A RID: 554 RVA: 0x000047BE File Offset: 0x000029BE
		public Profile Profile
		{
			get
			{
				return this._profile;
			}
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x0600022B RID: 555 RVA: 0x000047C6 File Offset: 0x000029C6
		public string ProfileName
		{
			get
			{
				return this._stProfileName;
			}
		}

		// Token: 0x040000BD RID: 189
		private Profile _profile;

		// Token: 0x040000BE RID: 190
		private string _stProfileName;
	}
}
