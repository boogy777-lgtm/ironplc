using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IEvaluationContext4 : IEvaluationContext3, IEvaluationContext2, IEvaluationContext
	{
		Guid LocalScopeIdentification { get; }
	}
}
