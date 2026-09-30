using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum IdentifierInfoFlag
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Variable = 1,
		[ReleasedEnumMember]
		Signature = 2,
		[ReleasedEnumMember]
		Scope = 4,
		[ReleasedEnumMember]
		Input = 8,
		[ReleasedEnumMember]
		Output = 0x10,
		[ReleasedEnumMember]
		Inout = 0x20,
		[ReleasedEnumMember]
		External = 0x40,
		[ReleasedEnumMember]
		Local = 0x80,
		[ReleasedEnumMember]
		Global = 0x100,
		[ReleasedEnumMember]
		Program = 0x200,
		[ReleasedEnumMember]
		Function = 0x400,
		[ReleasedEnumMember]
		Functionblock = 0x800,
		[ReleasedEnumMember]
		Action = 0x1000,
		[ReleasedEnumMember]
		Method = 0x2000,
		[ReleasedEnumMember]
		Keyword = 0x4000,
		[ReleasedEnumMember]
		Temporary = 0x8000,
		[ReleasedEnumMember]
		Static = 0x10000
	}
}
