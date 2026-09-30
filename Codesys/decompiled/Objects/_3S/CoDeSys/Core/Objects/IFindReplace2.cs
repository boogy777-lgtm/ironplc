using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000D2 RID: 210
	[ReleasedInterface]
	public interface IFindReplace2 : IFindReplace
	{
		// Token: 0x0600034C RID: 844
		SearchableTextBlock[] GetSearchableTextBlocks(long nPosition, int nLength);
	}
}
