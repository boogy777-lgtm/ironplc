using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum WriteExprementFlags : ulong
	{
		[ReleasedEnumMember]
		None = 0uL,
		[ReleasedEnumMember]
		MinimalParentheses = 1uL
	}
}
