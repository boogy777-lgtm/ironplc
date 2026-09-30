using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000D3 RID: 211
	[ReleasedInterface]
	public interface IFindReplace3 : IFindReplace2, IFindReplace
	{
		// Token: 0x0600034D RID: 845
		IMessage Replace2(long nPosition, int nLength, string stReplacement);
	}
}
