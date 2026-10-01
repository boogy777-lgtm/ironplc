using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000112 RID: 274
	[ReleasedInterface]
	public interface IPSAdd : IPSChange
	{
		// Token: 0x17000188 RID: 392
		// (get) Token: 0x06000433 RID: 1075
		IPastedObject PastedObject { get; }
	}
}
