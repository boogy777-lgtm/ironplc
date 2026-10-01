using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public enum EInterfaceOnlineVarRefState
	{
		[ReleasedEnumMember]
		NotYetMonitored,
		[ReleasedEnumMember]
		DynamicInstance,
		[ReleasedEnumMember]
		HiddenInstance,
		[ReleasedEnumMember]
		StaticInstancePathAvailable
	}
}
