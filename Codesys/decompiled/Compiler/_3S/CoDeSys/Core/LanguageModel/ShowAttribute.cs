using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{62bc566f-5835-4759-8a7f-648843daf3f9}")]
	public enum ShowAttribute : byte
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Precompile = 1,
		[ReleasedEnumMember]
		Compile = 2,
		[ReleasedEnumMember]
		All = byte.MaxValue
	}
}
