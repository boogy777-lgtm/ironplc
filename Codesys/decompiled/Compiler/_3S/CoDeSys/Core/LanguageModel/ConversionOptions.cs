using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	public enum ConversionOptions
	{
		[ReleasedEnumMember]
		TypesOnly,
		[ReleasedEnumMember]
		AddExplicitConversions,
		[ReleasedEnumMember]
		AddExplicitConversionsAndExplicitReferences
	}
}
