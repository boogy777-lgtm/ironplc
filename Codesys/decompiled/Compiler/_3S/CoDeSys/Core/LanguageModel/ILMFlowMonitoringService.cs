using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMFlowMonitoringService
	{
		IEnumerable<IFlowVarRef> GetAllFlowVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest);

		IEnumerable<IFlowVarRef> GetAllFlowVarReferences(string stInstance, long[] alPositionsOfInterest);

		IFlowVarRef GetFlowVarReference(string stExpression, string stInstancePath, long lPosition);
	}
}
