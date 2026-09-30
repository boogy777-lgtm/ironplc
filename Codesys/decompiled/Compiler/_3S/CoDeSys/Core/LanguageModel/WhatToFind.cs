using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	public enum WhatToFind
	{
		[ReleasedEnumMember]
		WholeInstancePath,
		[ReleasedEnumMember]
		PartialInstancePath,
		[ReleasedEnumMember]
		Statement,
		[ReleasedEnumMember]
		EnclosingExprement,
		[ReleasedEnumMember]
		ExactInstancePath,
		[ReleasedEnumMember]
		EnclosingCall,
		[ReleasedEnumMember]
		CallExpression
	}
}
