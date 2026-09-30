using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[Flags]
	public enum CrossRefOccurence
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		LanguageModel = 1,
		[ReleasedEnumMember]
		GeneratedInDownload = 2,
		[ReleasedEnumMember]
		Virtual = 4
	}
}
