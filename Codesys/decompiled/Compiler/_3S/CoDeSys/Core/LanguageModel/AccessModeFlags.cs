using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{00e7fcd2-f0fd-47b8-a84c-e0fd82f99789}")]
	public enum AccessModeFlags : uint
	{
		[ReleasedEnumMember]
		Unknown = 0u,
		[ReleasedEnumMember]
		Read = 1u,
		[ReleasedEnumMember]
		ReadAddress = 5u,
		[ReleasedEnumMember]
		Write = 2u,
		[ReleasedEnumMember]
		Address = 4u,
		[ReleasedEnumMember]
		WriteAddress = 6u,
		[ReleasedEnumMember]
		Parameter = 8u,
		[ReleasedEnumMember]
		Set = 0x10u,
		[ReleasedEnumMember]
		Reset = 0x20u,
		[ReleasedEnumMember]
		Call = 0x40u,
		[ReleasedEnumMember]
		OutParameter = 0x80u,
		[ReleasedEnumMember]
		Declaration = 0x100u,
		[ReleasedEnumMember]
		Type = 0x200u,
		[ReleasedEnumMember]
		All = uint.MaxValue
	}
}
