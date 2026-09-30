using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.LMSets;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000248 RID: 584
	public class DownloadedApplicationService : ILMDownloadedApplicationService2, ILMDownloadedApplicationService
	{
		// Token: 0x17000AF6 RID: 2806
		// (get) Token: 0x06002705 RID: 9989 RVA: 0x00061FEF File Offset: 0x00060FEF
		public IEnumerable<ILMCompiledApplicationSet> CompiledApplicationSets
		{
			get
			{
				return APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.DownloadedApplicationSets;
			}
		}

		// Token: 0x06002706 RID: 9990 RVA: 0x00062005 File Offset: 0x00061005
		public ILMCompiledApplicationSet GetCompiledApplicationSet(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
		}

		// Token: 0x06002707 RID: 9991 RVA: 0x00046C63 File Offset: 0x00045C63
		public void LoadDownloadedApplicationSetInBackground(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.LoadDownloadedApplicationSetInBackground(guidApplication);
		}

		// Token: 0x06002708 RID: 9992 RVA: 0x00061588 File Offset: 0x00060588
		public void GetCompiledIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.GetCompiledIds(guidApplication, out guidCodeId, out guidDataId);
		}

		// Token: 0x06002709 RID: 9993 RVA: 0x0006201C File Offset: 0x0006101C
		public void GetDownloadIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId)
		{
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
			if (icompileContext == null)
			{
				guidCodeId = Guid.Empty;
				guidDataId = Guid.Empty;
				return;
			}
			guidCodeId = icompileContext.CodeId;
			guidDataId = icompileContext.DataId;
		}

		// Token: 0x0600270A RID: 9994 RVA: 0x00062078 File Offset: 0x00061078
		public void GetInitialDownloadIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId)
		{
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
			if (icompileContext == null)
			{
				guidCodeId = Guid.Empty;
				guidDataId = Guid.Empty;
				return;
			}
			guidCodeId = icompileContext.LastCodeId;
			guidDataId = icompileContext.LastDataId;
		}

		// Token: 0x0600270B RID: 9995 RVA: 0x000620D4 File Offset: 0x000610D4
		public IEnumerable<IExternalReference> GetExternalReferences(Guid guidApplication, bool bCompactDownload)
		{
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
			if (icompileContext != null)
			{
				return CompilerProxy.GetExternalReferences(icompileContext, bCompactDownload);
			}
			return new IExternalReference[0];
		}

		// Token: 0x0600270C RID: 9996 RVA: 0x00062110 File Offset: 0x00061110
		public IVariable GetVariable(string stVariableName)
		{
			if (stVariableName == null)
			{
				throw new ArgumentNullException("stVariableName");
			}
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stVariableName, false, false, false, false);
			scanner.AllowMultipleUnderlines = true;
			IToken token;
			if (scanner.GetNext(out token) != TokenType.Identifier)
			{
				throw new ArgumentException(Strings.ErrExpressionContainsNoResource);
			}
			string text = scanner.GetIdentifier(token);
			IToken token2;
			if (scanner.GetNext(out token2) == TokenType.Operator && scanner.GetOperator(token2) == Operator.Period && scanner.GetNext(out token) == TokenType.Identifier)
			{
				string identifier = scanner.GetIdentifier(token);
				text = text + "." + identifier;
			}
			else
			{
				scanner.SetPosition(token2);
			}
			Guid applicationGuidByName = APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationGuidByName(text);
			if (scanner.GetNext(out token) != TokenType.Operator || scanner.GetOperator(token) != Operator.Period)
			{
				throw new ArgumentException(Strings.ErrExpressionContainsNoAppName);
			}
			bool flag;
			_IExpression iexpression = CompilerProxy.CreateParser(scanner).ParseSTOperand(out flag);
			if (flag)
			{
				throw new ArgumentException(Strings.ErrInvalidExpression);
			}
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(applicationGuidByName);
			if (icompileContext == null)
			{
				icompileContext = APEnvironmentFacade.Instance.LanguageModelMgr[applicationGuidByName];
			}
			if (icompileContext == null)
			{
				throw new ArgumentException(Strings.ErrNoApplication);
			}
			IScope scope = CompilerProxy.CreateGlobalScope(icompileContext);
			CompilerProxy.TypifyExprement(iexpression, scope, icompileContext, null, false, false, null);
			return iexpression.GetVariable(scope);
		}

		// Token: 0x0600270D RID: 9997 RVA: 0x00062260 File Offset: 0x00061260
		public bool IsUpToDate(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoader.CompleteLanguageModel(null);
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
			return icompileContext != null && icompileContext.IsUpToDate();
		}

		// Token: 0x0600270E RID: 9998 RVA: 0x000622A8 File Offset: 0x000612A8
		public bool IsUpToDate(Guid guidApplication, out bool bOnlineChangePossible)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoader.CompleteLanguageModel(null);
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
			bOnlineChangePossible = false;
			if (icompileContext == null)
			{
				return false;
			}
			_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(guidApplication);
			bool? flag = (ipreCompileContext != null) ? new bool?(ipreCompileContext.SimulationMode) : null;
			bool simulationMode = icompileContext.SimulationMode;
			return (flag.GetValueOrDefault() == simulationMode & flag != null) && icompileContext.IsUpToDate(ipreCompileContext, APEnvironmentFacade.Instance.LanguageModelMgr.Pool, out bOnlineChangePossible);
		}

		// Token: 0x0600270F RID: 9999 RVA: 0x00062350 File Offset: 0x00061350
		public ILMCompiledApplicationTypification GetTypificator(Guid guidApplication)
		{
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
			if (icompileContext == null)
			{
				return null;
			}
			return new CompiledApplicationTypification(icompileContext);
		}

		// Token: 0x06002710 RID: 10000 RVA: 0x00062384 File Offset: 0x00061384
		public ILMCompiledApplicationQuery QueryCompiledApplicationSet(Guid guidApplication)
		{
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
			if (icompileContext == null)
			{
				return null;
			}
			return new CompiledApplicationQuery(icompileContext);
		}

		// Token: 0x06002711 RID: 10001 RVA: 0x000623B8 File Offset: 0x000613B8
		public ILMCompiledApplicationDebugging GetApplicationDebugger(Guid guidApplication)
		{
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
			if (icompileContext == null)
			{
				return null;
			}
			return new CompiledApplicationDebugging(icompileContext);
		}

		// Token: 0x06002712 RID: 10002 RVA: 0x000623EC File Offset: 0x000613EC
		public ILMCompiledApplicationContentDumper GetCompiledApplicationContentDumper(Guid guidApplication)
		{
			_ICompileContext icompileContext = (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication);
			if (icompileContext == null)
			{
				return null;
			}
			return new CompiledApplicationContentDumper(icompileContext);
		}
	}
}
