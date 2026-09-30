using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum ETokenTextFlags
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		TruncateAtLinebreak = 1
	}
}
