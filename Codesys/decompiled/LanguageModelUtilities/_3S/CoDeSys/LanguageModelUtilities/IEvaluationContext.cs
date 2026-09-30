using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IEvaluationContext
	{
		Guid ScopeIdentification { get; }

		Guid ApplicationGuid { get; }
	}
}
