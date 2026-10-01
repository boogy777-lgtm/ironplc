using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200012A RID: 298
	[ReleasedInterface]
	public interface IPastedObject3 : IPastedObject2, IPastedObject
	{
		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x060004A1 RID: 1185
		ActionContext ActionContext { get; }
	}
}
