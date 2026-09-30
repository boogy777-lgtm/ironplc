using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[Flags]
	[SuppressMessage("Minor Code Smell", "S2342:Enumeration types should comply with a naming convention", Justification = "The proposed naming convention does not make sense in this case")]
	public enum EConstantFoldingResult
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		RecursionError = 1,
		[ReleasedEnumMember]
		Overflow = 2
	}
}
