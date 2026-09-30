using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum IntellisenseScopeInfoFlags
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		LocalScope = 1,
		[ReleasedEnumMember]
		GlobalScope = 2,
		[ReleasedEnumMember]
		KeywordScope = 4
	}
}
