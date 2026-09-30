using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000D6 RID: 214
	[ReleasedInterface]
	public interface IFreezableSelection
	{
		// Token: 0x06000351 RID: 849
		void SetSelectionFreeze(bool freeze, SelectionContext sc);

		// Token: 0x06000352 RID: 850
		bool IsSelectionFrozen(SelectionContext sc);
	}
}
