using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000101 RID: 257
	[ReleasedInterface]
	public interface IProjectConverterListener
	{
		// Token: 0x060003F7 RID: 1015
		void NotifyConverted(IProjectConverter converter, int nProjectHandle);
	}
}
