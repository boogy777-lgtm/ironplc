using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities2 : IPreCompileUtilities
	{
		IEvaluationContext2 CreateContext2(int nProj, int nAttrProj, Guid gdScope);
	}
}
