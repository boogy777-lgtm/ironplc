using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum ELMPreCompileSetType
	{
		[ReleasedEnumMember]
		Undefined = 0,
		[ReleasedEnumMember]
		Pool = 1,
		[ReleasedEnumMember]
		Libraries = 2,
		[ReleasedEnumMember]
		Devices = 4,
		[ReleasedEnumMember]
		All = 7
	}
}
