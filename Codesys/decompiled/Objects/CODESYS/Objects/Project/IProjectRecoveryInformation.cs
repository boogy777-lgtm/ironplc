using System;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.Objects.Project
{
	// Token: 0x02000005 RID: 5
	[ReleasedInterface]
	public interface IProjectRecoveryInformation
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000020 RID: 32
		string ProjectName { get; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000021 RID: 33
		string ProjectDirectory { get; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000022 RID: 34
		Guid[] ProjectAttributes { get; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000023 RID: 35
		string[] Auxiliaries { get; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000024 RID: 36
		string ProfileName { get; }
	}
}
