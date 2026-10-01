using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[TypeGuid("{6a1c5fc0-9632-4375-bbca-5dbf70430b06}")]
	public enum DirectVariableLocation
	{
		[ReleasedEnumMember]
		None,
		[ReleasedEnumMember]
		Input,
		[ReleasedEnumMember]
		Output,
		[ReleasedEnumMember]
		Memory
	}
}
