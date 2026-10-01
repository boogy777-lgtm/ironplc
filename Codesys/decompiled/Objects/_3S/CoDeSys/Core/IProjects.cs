using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000024 RID: 36
	[ReleasedInterface]
	public interface IProjects
	{
		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600009E RID: 158
		IProject PrimaryProject { get; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600009F RID: 159
		IProject[] Projects { get; }

		// Token: 0x060000A0 RID: 160
		IProject CreateProject(string stProjectLocation, string stProjectName, params Guid[] projectAttrs);

		// Token: 0x060000A1 RID: 161
		IProject OpenProject(string stPath, params Guid[] projectAttrs);

		// Token: 0x060000A2 RID: 162
		IProject ImportProject(string stInputPath, string stOutputPath, Guid projectConverterFactoryGuid, params Guid[] projectAttrs);

		// Token: 0x060000A3 RID: 163
		IProject GetProjectByHandle(int nProjectHandle);

		// Token: 0x060000A4 RID: 164
		IProject GetProjectByPath(string stPath);

		// Token: 0x060000A5 RID: 165
		IProject[] GetProjectsByAttributes(params Guid[] projectAttrs);

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x060000A6 RID: 166
		// (remove) Token: 0x060000A7 RID: 167
		event PrimaryProjectSwitchedEventHandler PrimaryProjectSwitched;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x060000A8 RID: 168
		// (remove) Token: 0x060000A9 RID: 169
		event PrimaryProjectSwitchedEventHandler BeforePrimaryProjectSwitched;

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x060000AA RID: 170
		// (remove) Token: 0x060000AB RID: 171
		event PrimaryProjectSwitchedEventHandler AfterPrimaryProjectSwitched;
	}
}
