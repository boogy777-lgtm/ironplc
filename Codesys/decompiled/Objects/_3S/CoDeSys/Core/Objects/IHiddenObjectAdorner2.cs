using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000147 RID: 327
	[ReleasedInterface]
	public interface IHiddenObjectAdorner2 : IHiddenObjectAdorner
	{
		// Token: 0x060004EA RID: 1258
		bool ShouldBeHiddenWithContext(HiddenObjectAdornerParameters hoaParams);
	}
}
