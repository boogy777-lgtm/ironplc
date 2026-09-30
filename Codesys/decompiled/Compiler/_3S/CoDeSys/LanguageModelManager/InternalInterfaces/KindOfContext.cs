using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[TypeGuid("{1d8fe3d0-8423-48cf-8a9b-eaa134990850}")]
	public enum KindOfContext
	{
		[ReleasedEnumMember]
		None,
		[ReleasedEnumMember]
		Target,
		[ReleasedEnumMember]
		Pool,
		[ReleasedEnumMember]
		Library
	}
}
