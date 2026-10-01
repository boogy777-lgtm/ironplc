using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[SuppressMessage("Minor Code Smell", "S2344:Enumeration type names should not have \"Flags\" or \"Enum\" suffixes", Justification = "Useless check. Name has been chosen intentionally according to already existing enum names")]
	public enum EPouScopeFlags
	{
		[ReleasedEnumMember]
		Declaration = 1,
		[ReleasedEnumMember]
		Implementation = 2
	}
}
