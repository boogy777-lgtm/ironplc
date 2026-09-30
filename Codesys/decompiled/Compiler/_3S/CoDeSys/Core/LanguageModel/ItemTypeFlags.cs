using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum ItemTypeFlags
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		NameSpaceItem = 1,
		[ReleasedEnumMember]
		OperatorItem = 2,
		[ReleasedEnumMember]
		ConversionItem = 4,
		[ReleasedEnumMember]
		KeywordItem = 8,
		[ReleasedEnumMember]
		StandardDatatypeItem = 0x10
	}
}
