using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[Flags]
	[TypeGuid("{be5c129c-a197-417b-85f0-00907e2c063f}")]
	public enum AreaFlags : uint
	{
		[ReleasedEnumMember]
		None = 0u,
		[ReleasedEnumMember]
		Automatic = 1u,
		[ReleasedEnumMember]
		DynamicSize = 4u,
		[ReleasedEnumMember]
		OnlineChange = 8u,
		[ReleasedEnumMember]
		Fixed = 0x10u,
		[ReleasedEnumMember]
		NoUse = 0x20u,
		[ReleasedEnumMember]
		MappedSegmentAccessOnly = 0x80000000u,
		[ReleasedEnumMember]
		AllDefault = 0x7FFFFFFFu
	}
}
