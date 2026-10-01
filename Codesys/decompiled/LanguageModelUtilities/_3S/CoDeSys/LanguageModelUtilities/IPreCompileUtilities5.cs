using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities5 : IPreCompileUtilities4, IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		IEvaluationContext3 CreateAppSpecificContext3(int nProj, int nAttrProj, Guid gdApp, Guid gdScope, IGetLibInformation libInfo);
	}
}
