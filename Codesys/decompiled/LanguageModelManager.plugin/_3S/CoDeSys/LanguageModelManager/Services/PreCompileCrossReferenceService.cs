using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x0200024C RID: 588
	public class PreCompileCrossReferenceService : ILMPreCompileCrossReferenceService
	{
		// Token: 0x0600272C RID: 10028 RVA: 0x000626D4 File Offset: 0x000616D4
		public void MarkApplicationsDirty()
		{
			foreach (IPreCompileContext preCompileContext in APEnvironmentFacade.Instance.LanguageModelMgr._AllPreCompileContexts(true, false))
			{
				(preCompileContext as _IPreCompileContext).Dirty = true;
			}
		}

		// Token: 0x17000AF7 RID: 2807
		// (get) Token: 0x0600272D RID: 10029 RVA: 0x00062730 File Offset: 0x00061730
		public bool PrecompileInformationUpToDate
		{
			get
			{
				return !APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoaderWorking && CompilerProxy.PrecompileChecksDone();
			}
		}

		// Token: 0x0600272E RID: 10030 RVA: 0x0006274A File Offset: 0x0006174A
		public void FinishPrecompileChecks()
		{
			CompilerProxy.GetCheckerThread().FinishPrecompileChecks();
		}

		// Token: 0x0600272F RID: 10031 RVA: 0x00062756 File Offset: 0x00061756
		public void EnablePrecompileChecksInNoUIMode()
		{
			CompilerProxy.GetCheckerThread().EnablePrecompileChecksInNoUIMode();
		}

		// Token: 0x06002730 RID: 10032 RVA: 0x00062762 File Offset: 0x00061762
		public void DisablePrecompileChecksInNoUIMode()
		{
			CompilerProxy.GetCheckerThread().DisablePrecompileChecksInNoUIMode();
		}

		// Token: 0x06002731 RID: 10033 RVA: 0x00062770 File Offset: 0x00061770
		public IDictionary<string, IList<IAccessInfo>> GetPrecompiledCrossReferences(Regex regex)
		{
			CaseInsensitiveDictionary<IList<IAccessInfo>> caseInsensitiveDictionary = new CaseInsensitiveDictionary<IList<IAccessInfo>>();
			if (regex.ToString().StartsWith("%"))
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.PCCRDirVars.GetCrossReferences(regex, caseInsensitiveDictionary);
			}
			else
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.PCCRVariables.GetCrossReferences(regex, caseInsensitiveDictionary);
				APEnvironmentFacade.Instance.LanguageModelMgr.PCCRCalls.GetCrossReferences(regex, caseInsensitiveDictionary);
			}
			return caseInsensitiveDictionary;
		}

		// Token: 0x06002732 RID: 10034 RVA: 0x000627DA File Offset: 0x000617DA
		public bool CheckAllPOUs(ILMPreCompileSet precompileSet)
		{
			return ((_IPreCompileContext)precompileSet).CheckAllPOUs();
		}

		// Token: 0x06002733 RID: 10035 RVA: 0x000627E7 File Offset: 0x000617E7
		public IList<IPrecompilePositionInfo> GetVariableReferencePositions(ILMPreCompileSet setOfSignature, int nSignatureWithReferencesPrecompileId, int nSignatureWithVarPrecompileId, int nVariablePrecompileId)
		{
			return ((_IPreCompileContext)setOfSignature).GetVariableReferencePositions(nSignatureWithReferencesPrecompileId, nSignatureWithVarPrecompileId, nVariablePrecompileId);
		}

		// Token: 0x06002734 RID: 10036 RVA: 0x000627F8 File Offset: 0x000617F8
		public IList<IPrecompilePositionInfo> GetCrossReferencePositions(ILMPreCompileSet setOfSignature, int nCallerSignaturePrecompileId, int nCalleeSignaturePrecompileId)
		{
			return ((_IPreCompileContext)setOfSignature).GetCrossReferencePositions(nCallerSignaturePrecompileId, nCalleeSignaturePrecompileId);
		}

		// Token: 0x06002735 RID: 10037 RVA: 0x00062807 File Offset: 0x00061807
		public IList<IPrecompilePositionInfo> GetDirectCrossReferencePositions(ILMPreCompileSet setOfSignature, int nCallerSignaturePrecompileId, int nCalleeSignaturePrecompileId)
		{
			return ((_IPreCompileContext)setOfSignature).GetDirectCrossReferencePositions(nCallerSignaturePrecompileId, nCalleeSignaturePrecompileId);
		}

		// Token: 0x06002736 RID: 10038 RVA: 0x00062816 File Offset: 0x00061816
		public IEnumerable<IAccessInfo> GetDirectVariableAccess(IDirectVariable dirvar)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.PCCRDirVars[dirvar.ToString()];
		}

		// Token: 0x06002737 RID: 10039 RVA: 0x00062832 File Offset: 0x00061832
		public IEnumerable<IDirectVariableAccess> GetAllDirectVariableAccesses()
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.PCCRDirVars.AllAccesses();
		}

		// Token: 0x06002738 RID: 10040 RVA: 0x00062848 File Offset: 0x00061848
		public IEnumerable<IAccessInfo> GetVariableAccess(string stVariableName)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.PCCRVariables[stVariableName];
		}

		// Token: 0x06002739 RID: 10041 RVA: 0x0006285F File Offset: 0x0006185F
		public IEnumerable<IAccessInfo> GetPOUAccess(string stPOUName)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.PCCRCalls[stPOUName];
		}
	}
}
