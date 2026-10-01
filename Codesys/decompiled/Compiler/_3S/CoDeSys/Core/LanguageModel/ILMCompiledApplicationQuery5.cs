using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationQuery5 : ILMCompiledApplicationQuery4, ILMCompiledApplicationQuery3, ILMCompiledApplicationQuery2, ILMCompiledApplicationQuery
	{
		IEnumerable<IInstancePathInfoWithAttribute> InstancePathsForAttribute(string stAttributeName, Guid gdApplication);

		IEnumerable<IInstancePathInfoWithAttribute> InstancePathsForAttribute(string stAttributeName, Guid gdApplication, EInstancePathsLookupFlag eLookupFlags);
	}
}
