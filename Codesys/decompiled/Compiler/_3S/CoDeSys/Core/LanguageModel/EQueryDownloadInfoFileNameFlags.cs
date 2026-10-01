using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum EQueryDownloadInfoFileNameFlags
	{
		[ReleasedEnumMember]
		Default = 0,
		[ReleasedEnumMember]
		Simulation = 1,
		[ReleasedEnumMember]
		Precompile = 2
	}
}
