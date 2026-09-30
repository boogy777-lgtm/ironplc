using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F9 RID: 249
	[ReleasedInterface]
	public interface IProjectInspectionReporter
	{
		// Token: 0x060003EA RID: 1002
		bool ReportMissingTypes(IEnumerable<MissingTypeInformation2> missingTypes, string stProjectPath, string stStorageProfileName);
	}
}
