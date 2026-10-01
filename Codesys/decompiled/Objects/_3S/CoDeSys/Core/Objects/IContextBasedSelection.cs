using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000D4 RID: 212
	[ReleasedInterface]
	public interface IContextBasedSelection
	{
		// Token: 0x0600034E RID: 846
		void Select(SelectionContext sc, long nPosition, int nLength);

		// Token: 0x0600034F RID: 847
		void GetSelection(SelectionContext sc, out long nPosition, out int nLength);
	}
}
