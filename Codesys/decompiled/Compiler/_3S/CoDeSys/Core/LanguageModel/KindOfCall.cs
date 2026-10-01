using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[TypeGuid("{fcb43bca-9283-4080-8c7a-085c4f21024c}")]
	public enum KindOfCall : byte
	{
		[ReleasedEnumMember]
		ProgramCall,
		[ReleasedEnumMember]
		StaticFunctionCall,
		[ReleasedEnumMember]
		VirtualFunctionCall,
		[ReleasedEnumMember]
		InterfaceCall,
		[ReleasedEnumMember]
		None
	}
}
