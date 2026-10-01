using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000DD RID: 221
	[ReleasedInterface]
	public interface IProjectPlugInInformation
	{
		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000372 RID: 882
		string PlugInName { get; }

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000373 RID: 883
		Version PlugInVersion { get; }

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000374 RID: 884
		IEnumerable<Guid> TypeGuids { get; }
	}
}
