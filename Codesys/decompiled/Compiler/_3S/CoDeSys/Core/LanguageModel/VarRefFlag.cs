using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum VarRefFlag
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Constant = 1,
		[ReleasedEnumMember]
		Extensible = 2,
		[ReleasedEnumMember]
		Invalid = 4,
		[ReleasedEnumMember]
		Property = 8,
		[ReleasedEnumMember]
		PropertyByCall = 0x10,
		[ReleasedEnumMember]
		PropertyByVariable = 0x20,
		[ReleasedEnumMember]
		Flow = 0x40,
		[ReleasedEnumMember]
		FunctionByCall = 0x80,
		[ReleasedEnumMember]
		ReachedOnly = 0x100,
		[ReleasedEnumMember]
		Interface = 0x200
	}
}
