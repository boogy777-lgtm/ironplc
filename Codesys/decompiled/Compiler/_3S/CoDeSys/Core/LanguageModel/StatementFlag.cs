using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{f50f65ea-683d-4570-b91e-ebd8bef193eb}")]
	public enum StatementFlag : long
	{
		[ReleasedEnumMember]
		GenerateFlow = 1L,
		[ReleasedEnumMember]
		GenerateBP = 2L,
		[ReleasedEnumMember]
		Implicit = 4L,
		[ReleasedEnumMember]
		Library = 8L,
		[ReleasedEnumMember]
		TypifiedNotChecked = 0x10L,
		[ReleasedEnumMember]
		GenerateBP2 = 0x20L,
		[ReleasedEnumMember]
		Unused = 0x40L
	}
}
