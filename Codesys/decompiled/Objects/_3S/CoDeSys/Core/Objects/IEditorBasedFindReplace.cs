using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000D0 RID: 208
	[ReleasedInterface]
	public interface IEditorBasedFindReplace
	{
		// Token: 0x06000349 RID: 841
		bool UndoableReplace(long nPosition, int nLength, string stReplacement);
	}
}
