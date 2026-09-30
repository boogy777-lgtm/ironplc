using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000249 RID: 585
	public class FlowMonitoringService : ILMFlowMonitoringService
	{
		// Token: 0x06002714 RID: 10004 RVA: 0x0006241F File Offset: 0x0006141F
		public IEnumerable<IFlowVarRef> GetAllFlowVarReferences(string stInstance, long[] alPositionsOfInterest)
		{
			return VarReferenceCreator.GetAllFlowVarReferences(stInstance, alPositionsOfInterest);
		}

		// Token: 0x06002715 RID: 10005 RVA: 0x00062428 File Offset: 0x00061428
		public IEnumerable<IFlowVarRef> GetAllFlowVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest)
		{
			return VarReferenceCreator.GetAllFlowVarReferences(objectguid, guidExplicitApplicationGuid, stInstance, alPositionsOfInterest);
		}

		// Token: 0x06002716 RID: 10006 RVA: 0x00062434 File Offset: 0x00061434
		public IFlowVarRef GetFlowVarReference(string stExpression, string stInstancePath, long lPosition)
		{
			return VarReferenceCreator.GetFlowVarReference(stExpression, stInstancePath, lPosition);
		}
	}
}
