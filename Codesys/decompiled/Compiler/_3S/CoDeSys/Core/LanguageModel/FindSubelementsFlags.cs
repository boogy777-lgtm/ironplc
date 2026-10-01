using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum FindSubelementsFlags
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		IncludeLocalVars = 1,
		[ReleasedEnumMember]
		IncludeArrayElements = 2,
		[ReleasedEnumMember]
		SearchForType = 4,
		[ReleasedEnumMember]
		IncludeTypeSubElements = 8,
		[ReleasedEnumMember]
		ExcludeVarTemp = 0x10,
		[ReleasedEnumMember]
		InputsOnly = 0x20,
		[ReleasedEnumMember]
		ExcludeSubSignatures = 0x40,
		[ReleasedEnumMember]
		ExcludeProperties = 0x80,
		[ReleasedEnumMember]
		OutputsOnly = 0x200,
		[ReleasedEnumMember]
		ExcludePropertyGetter = 0x400,
		[ReleasedEnumMember]
		ExcludePropertySetter = 0x800
	}
}
