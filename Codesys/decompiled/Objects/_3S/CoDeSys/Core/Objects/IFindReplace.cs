using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000D1 RID: 209
	[ReleasedInterface]
	public interface IFindReplace
	{
		// Token: 0x0600034A RID: 842
		SearchableTextBlock[] GetSearchableTextBlocks();

		// Token: 0x0600034B RID: 843
		void Replace(long nPosition, int nLength, string stReplacement);
	}
}
