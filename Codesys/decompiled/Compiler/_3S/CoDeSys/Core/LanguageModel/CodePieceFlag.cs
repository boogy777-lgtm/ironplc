using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum CodePieceFlag : ushort
	{
		[ReleasedEnumMember]
		Changed = 1
	}
}
