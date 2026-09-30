using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	[TypeGuid("{79AA0CCE-71E9-4699-9F63-03CF09DCC367}")]
	public enum DataLocationFlag
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		RelativeInstance = 1,
		[ReleasedEnumMember]
		RelativeStack = 2
	}
}
