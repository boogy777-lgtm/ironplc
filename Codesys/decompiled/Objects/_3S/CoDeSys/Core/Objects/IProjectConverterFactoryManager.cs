using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000FF RID: 255
	[ReleasedInterface]
	public interface IProjectConverterFactoryManager
	{
		// Token: 0x1700017D RID: 381
		// (get) Token: 0x060003F1 RID: 1009
		IProjectConverterFactory[] Factories { get; }

		// Token: 0x060003F2 RID: 1010
		IProjectConverterFactory GetFactory(Guid factoryGuid);
	}
}
