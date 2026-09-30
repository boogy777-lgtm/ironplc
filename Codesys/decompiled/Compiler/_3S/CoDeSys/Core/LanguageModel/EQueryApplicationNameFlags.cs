using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum EQueryApplicationNameFlags
	{
		[ReleasedEnumMember]
		Default = 0,
		[ReleasedEnumMember]
		SimulationMode = 1,
		[ReleasedEnumMember]
		Unqualified = 2
	}
}
