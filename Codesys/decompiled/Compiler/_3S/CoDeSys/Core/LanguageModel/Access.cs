using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	public enum Access : byte
	{
		[ReleasedEnumMember]
		Read = 0,
		[ReleasedEnumMember]
		Write = 1,
		[ReleasedEnumMember]
		Set = 2,
		[ReleasedEnumMember]
		Reset = 3,
		[ReleasedEnumMember]
		NONE = byte.MaxValue
	}
}
