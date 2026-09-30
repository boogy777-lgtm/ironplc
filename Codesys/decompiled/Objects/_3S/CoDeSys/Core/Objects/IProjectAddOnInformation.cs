using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F0 RID: 240
	[ReleasedInterface]
	public interface IProjectAddOnInformation
	{
		// Token: 0x17000173 RID: 371
		// (get) Token: 0x060003A7 RID: 935
		Guid PackageId { get; }

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x060003A8 RID: 936
		string PackageName { get; }

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x060003A9 RID: 937
		Version PackageVersion { get; }

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x060003AA RID: 938
		IEnumerable<IProjectPlugInInformation> PlugIns { get; }

		// Token: 0x060003AB RID: 939
		bool Contains(Guid typeGuid);
	}
}
