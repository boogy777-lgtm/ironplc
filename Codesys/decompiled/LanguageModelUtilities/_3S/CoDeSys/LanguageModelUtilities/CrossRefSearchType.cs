using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[Flags]
	public enum CrossRefSearchType
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Signatures = 1,
		[ReleasedEnumMember]
		Variables = 2,
		[ReleasedEnumMember]
		Objects = 3
	}
}
