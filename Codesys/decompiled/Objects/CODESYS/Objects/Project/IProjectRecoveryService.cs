using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace CODESYS.Objects.Project
{
	// Token: 0x02000003 RID: 3
	[ReleasedInterface]
	public interface IProjectRecoveryService
	{
		// Token: 0x0600001B RID: 27
		void CreateSnapshot(int nProjectHandle, string stRecoveryDirectory, bool bCopyInsteadOfMove);

		// Token: 0x0600001C RID: 28
		int RecoverProject(string stRecoveryDirectory);

		// Token: 0x0600001D RID: 29
		int RecoverProject(IProjectSource projectSource, string stRecoveryDirectory);

		// Token: 0x0600001E RID: 30
		string GetPath(string stRecoveryDirectory, ProjectRecoveryFiles file);

		// Token: 0x0600001F RID: 31
		IProjectRecoveryInformation ReadRecoveryInformation(string stRecoveryDirectory);
	}
}
