using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	public enum RegisterType : byte
	{
		[ReleasedEnumMember]
		GPReg,
		[ReleasedEnumMember]
		FPUReg,
		[ReleasedEnumMember]
		ADRReg
	}
}
