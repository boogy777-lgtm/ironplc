using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{da562b59-8dbe-45d3-bf53-ee66b4292d64}")]
	public enum AccessFlag : byte
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Read = 1,
		[ReleasedEnumMember]
		Write = 2,
		[ReleasedEnumMember]
		Call = 4,
		[ReleasedEnumMember]
		Implicit = 8,
		[ReleasedEnumMember]
		Declarative = 0x10,
		[ReleasedEnumMember]
		Unknown = 0x20,
		[ReleasedEnumMember]
		Address = 0x40,
		[ReleasedEnumMember]
		Type = 0x80,
		[ReleasedEnumMember]
		InitializingWrite = 0x81
	}
}
