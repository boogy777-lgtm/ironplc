using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{68cddacd-2830-4780-ae45-869af5892c4f}")]
	public enum DataSegmentFlags : ushort
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Data = 1,
		[ReleasedEnumMember]
		Constant = 2,
		[ReleasedEnumMember]
		Input = 4,
		[ReleasedEnumMember]
		Output = 8,
		[ReleasedEnumMember]
		Memory = 0x10,
		[ReleasedEnumMember]
		Retain = 0x20,
		[ReleasedEnumMember]
		Code = 0x40,
		[ReleasedEnumMember]
		Area = 0x80,
		[ReleasedEnumMember]
		Persistent = 0x100,
		[ReleasedEnumMember]
		NonSafety = 0x800,
		[ReleasedEnumMember]
		Special = 0x8000,
		[ReleasedEnumMember]
		All = ushort.MaxValue
	}
}
