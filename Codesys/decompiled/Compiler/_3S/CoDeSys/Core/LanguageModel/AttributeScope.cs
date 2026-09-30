using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum AttributeScope
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Variable = 1,
		[ReleasedEnumMember]
		Signature = 2,
		[ReleasedEnumMember]
		All = 3
	}
}
