using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000FD RID: 253
	[ReleasedInterface]
	public interface IProjectConverterFactory
	{
		// Token: 0x1700017A RID: 378
		// (get) Token: 0x060003ED RID: 1005
		string Name { get; }

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x060003EE RID: 1006
		string FileFilter { get; }

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x060003EF RID: 1007
		ProjectType ProjectType { get; }

		// Token: 0x060003F0 RID: 1008
		IProjectConverter Create();
	}
}
