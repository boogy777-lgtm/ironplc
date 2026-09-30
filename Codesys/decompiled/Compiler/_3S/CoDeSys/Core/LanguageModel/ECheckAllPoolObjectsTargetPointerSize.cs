using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum ECheckAllPoolObjectsTargetPointerSize
	{
		[ReleasedEnumMember]
		PointerSize4 = 4,
		[ReleasedEnumMember]
		PointerSize8 = 8,
		[ReleasedEnumMember]
		PointerSize4And8 = 0xC
	}
}
