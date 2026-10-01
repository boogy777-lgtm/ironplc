using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum EMetricFlagsDuringDownload
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Optional = 1
	}
}
