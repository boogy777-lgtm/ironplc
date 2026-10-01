using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.LMSets;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000245 RID: 581
	public class CompileService : ILMCompileService3, ILMCompileService2, ILMCompileService
	{
		// Token: 0x1400003C RID: 60
		// (add) Token: 0x060026A8 RID: 9896 RVA: 0x00060B40 File Offset: 0x0005FB40
		// (remove) Token: 0x060026A9 RID: 9897 RVA: 0x00060B78 File Offset: 0x0005FB78
		public event AddImplicitCodeEventHandler AddDownloadCode;

		// Token: 0x1400003D RID: 61
		// (add) Token: 0x060026AA RID: 9898 RVA: 0x00060BB0 File Offset: 0x0005FBB0
		// (remove) Token: 0x060026AB RID: 9899 RVA: 0x00060BE8 File Offset: 0x0005FBE8
		public event AddImplicitCodeEventHandler AddGlobalInitCode;

		// Token: 0x1400003E RID: 62
		// (add) Token: 0x060026AC RID: 9900 RVA: 0x00060C20 File Offset: 0x0005FC20
		// (remove) Token: 0x060026AD RID: 9901 RVA: 0x00060C58 File Offset: 0x0005FC58
		public event AddLanguageModelEventHandler AddLateLanguageModel;

		// Token: 0x1400003F RID: 63
		// (add) Token: 0x060026AE RID: 9902 RVA: 0x00060C90 File Offset: 0x0005FC90
		// (remove) Token: 0x060026AF RID: 9903 RVA: 0x00060CC8 File Offset: 0x0005FCC8
		public event AddImplicitCodeEventHandler AddOnlineChangeCode;

		// Token: 0x14000040 RID: 64
		// (add) Token: 0x060026B0 RID: 9904 RVA: 0x00060D00 File Offset: 0x0005FD00
		// (remove) Token: 0x060026B1 RID: 9905 RVA: 0x00060D38 File Offset: 0x0005FD38
		public event CompileEventHandler AfterCompile;

		// Token: 0x14000041 RID: 65
		// (add) Token: 0x060026B2 RID: 9906 RVA: 0x00060D70 File Offset: 0x0005FD70
		// (remove) Token: 0x060026B3 RID: 9907 RVA: 0x00060DA8 File Offset: 0x0005FDA8
		public event CompileEventHandler AfterGenerateCode;

		// Token: 0x14000042 RID: 66
		// (add) Token: 0x060026B4 RID: 9908 RVA: 0x00060DE0 File Offset: 0x0005FDE0
		// (remove) Token: 0x060026B5 RID: 9909 RVA: 0x00060E18 File Offset: 0x0005FE18
		public event EventHandler<AfterGenerateGlobalInitEventArgs> AfterGenerateGlobalInitCode;

		// Token: 0x14000043 RID: 67
		// (add) Token: 0x060026B6 RID: 9910 RVA: 0x00060E50 File Offset: 0x0005FE50
		// (remove) Token: 0x060026B7 RID: 9911 RVA: 0x00060E88 File Offset: 0x0005FE88
		public event CompileEventHandler AfterLocation;

		// Token: 0x14000044 RID: 68
		// (add) Token: 0x060026B8 RID: 9912 RVA: 0x00060EC0 File Offset: 0x0005FEC0
		// (remove) Token: 0x060026B9 RID: 9913 RVA: 0x00060EF8 File Offset: 0x0005FEF8
		public event AfterMessageOutputEventHandler AfterMessageOutput;

		// Token: 0x14000045 RID: 69
		// (add) Token: 0x060026BA RID: 9914 RVA: 0x00060F2D File Offset: 0x0005FF2D
		// (remove) Token: 0x060026BB RID: 9915 RVA: 0x00060F3B File Offset: 0x0005FF3B
		public event CompileEventHandler BeforeCompile
		{
			add
			{
				this._lmm.BeforeCompile += value;
			}
			remove
			{
				this._lmm.BeforeCompile -= value;
			}
		}

		// Token: 0x14000046 RID: 70
		// (add) Token: 0x060026BC RID: 9916 RVA: 0x00060F4C File Offset: 0x0005FF4C
		// (remove) Token: 0x060026BD RID: 9917 RVA: 0x00060F84 File Offset: 0x0005FF84
		public event EventHandler<CompileEventArgs> BeforeGenerateCompiledCode;

		// Token: 0x14000047 RID: 71
		// (add) Token: 0x060026BE RID: 9918 RVA: 0x00060FBC File Offset: 0x0005FFBC
		// (remove) Token: 0x060026BF RID: 9919 RVA: 0x00060FF4 File Offset: 0x0005FFF4
		public event EventHandler<BeforeGenerateRelinkCodeEventArgs> BeforeGenerateRelinkCode;

		// Token: 0x14000048 RID: 72
		// (add) Token: 0x060026C0 RID: 9920 RVA: 0x0006102C File Offset: 0x0006002C
		// (remove) Token: 0x060026C1 RID: 9921 RVA: 0x00061064 File Offset: 0x00060064
		public event CompileEventHandler BeforeLocation;

		// Token: 0x14000049 RID: 73
		// (add) Token: 0x060026C2 RID: 9922 RVA: 0x0006109C File Offset: 0x0006009C
		// (remove) Token: 0x060026C3 RID: 9923 RVA: 0x000610D4 File Offset: 0x000600D4
		public event BeforeMessageOutputEventHandler BeforeMessageOutput;

		// Token: 0x1400004A RID: 74
		// (add) Token: 0x060026C4 RID: 9924 RVA: 0x0006110C File Offset: 0x0006010C
		// (remove) Token: 0x060026C5 RID: 9925 RVA: 0x00061144 File Offset: 0x00060144
		public event CompileEventHandler CodeChanged;

		// Token: 0x1400004B RID: 75
		// (add) Token: 0x060026C6 RID: 9926 RVA: 0x0006117C File Offset: 0x0006017C
		// (remove) Token: 0x060026C7 RID: 9927 RVA: 0x000611B4 File Offset: 0x000601B4
		public event FilterMessageOutputEventHandler FilterMessageOutput;

		// Token: 0x1400004C RID: 76
		// (add) Token: 0x060026C8 RID: 9928 RVA: 0x000611EC File Offset: 0x000601EC
		// (remove) Token: 0x060026C9 RID: 9929 RVA: 0x00061224 File Offset: 0x00060224
		public event AfterUnsuccessfullGenerateCodeEventHandler AfterUnsuccessfullGenerateCode;

		// Token: 0x060026CA RID: 9930 RVA: 0x0006125C File Offset: 0x0006025C
		public CompileService(LanguageModelManagerConsolidated lmm)
		{
			this._lmm = lmm;
			lmm.AddDownloadCode += this.LanguageModelMgrOnAddDownloadCode;
			lmm.AddGlobalInitCode += this.LanguageModelMgrOnAddGlobalInitCode;
			lmm.AddLateLanguageModel += this.LanguageModelMgrOnAddLateLanguageModel;
			lmm.AddOnlineChangeCode += this.LanguageModelMgrOnAddOnlineChangeCode;
			lmm.AfterCompile += this.LanguageModelMgrOnAfterCompile;
			lmm.AfterGenerateCode += this.LanguageModelMgrOnAfterGenerateCode;
			lmm.AfterGenerateGlobalInitCode += this.LanguageModelMgrOnAfterGenerateGlobalInitCode;
			lmm.AfterLocation += this.LanguageModelMgrOnAfterLocation;
			lmm.AfterMessageOutput += this.LanguageModelMgrOnAfterMessageOutput;
			lmm.BeforeGenerateCompiledCode += this.LanguageModelMgrOnBeforeGenerateCompiledCode;
			lmm.BeforeGenerateRelinkCode += this.LanguageModelMgrOnBeforeGenerateRelinkCode;
			lmm.BeforeLocation += this.LanguageModelMgrOnBeforeLocation;
			lmm.BeforeMessageOutput += this.LanguageModelMgrOnBeforeMessageOutput;
			lmm.CodeChanged += this.LanguageModelMgrOnCodeChanged;
			lmm.FilterMessageOutput += this.LanguageModelMgrOnFilterMessageOutput;
		}

		// Token: 0x060026CB RID: 9931 RVA: 0x00061384 File Offset: 0x00060384
		private void LanguageModelMgrOnFilterMessageOutput(object sender, FilterMessageOutputEventArgs filterMessageOutputEventArgs)
		{
			FilterMessageOutputEventHandler filterMessageOutput = this.FilterMessageOutput;
			if (filterMessageOutput == null)
			{
				return;
			}
			filterMessageOutput(sender, filterMessageOutputEventArgs);
		}

		// Token: 0x060026CC RID: 9932 RVA: 0x00061398 File Offset: 0x00060398
		private void LanguageModelMgrOnCodeChanged(object sender, CompileEventArgs compileEventArgs)
		{
			CompileEventHandler codeChanged = this.CodeChanged;
			if (codeChanged == null)
			{
				return;
			}
			codeChanged(sender, compileEventArgs);
		}

		// Token: 0x060026CD RID: 9933 RVA: 0x000613AC File Offset: 0x000603AC
		private void LanguageModelMgrOnBeforeMessageOutput(object sender, MessageOutputEventArgs messageOutputEventArgs)
		{
			BeforeMessageOutputEventHandler beforeMessageOutput = this.BeforeMessageOutput;
			if (beforeMessageOutput == null)
			{
				return;
			}
			beforeMessageOutput(sender, messageOutputEventArgs);
		}

		// Token: 0x060026CE RID: 9934 RVA: 0x000613C0 File Offset: 0x000603C0
		private void LanguageModelMgrOnBeforeLocation(object sender, CompileEventArgs compileEventArgs)
		{
			CompileEventHandler beforeLocation = this.BeforeLocation;
			if (beforeLocation == null)
			{
				return;
			}
			beforeLocation(sender, compileEventArgs);
		}

		// Token: 0x060026CF RID: 9935 RVA: 0x000613D4 File Offset: 0x000603D4
		private void LanguageModelMgrOnBeforeGenerateRelinkCode(object sender, BeforeGenerateRelinkCodeEventArgs beforeGenerateRelinkCodeEventArgs)
		{
			EventHandler<BeforeGenerateRelinkCodeEventArgs> beforeGenerateRelinkCode = this.BeforeGenerateRelinkCode;
			if (beforeGenerateRelinkCode == null)
			{
				return;
			}
			beforeGenerateRelinkCode(sender, beforeGenerateRelinkCodeEventArgs);
		}

		// Token: 0x060026D0 RID: 9936 RVA: 0x000613E8 File Offset: 0x000603E8
		private void LanguageModelMgrOnBeforeGenerateCompiledCode(object sender, CompileEventArgs compileEventArgs)
		{
			EventHandler<CompileEventArgs> beforeGenerateCompiledCode = this.BeforeGenerateCompiledCode;
			if (beforeGenerateCompiledCode == null)
			{
				return;
			}
			beforeGenerateCompiledCode(sender, compileEventArgs);
		}

		// Token: 0x060026D1 RID: 9937 RVA: 0x000613FC File Offset: 0x000603FC
		private void LanguageModelMgrOnAfterMessageOutput(object sender, MessageOutputEventArgs messageOutputEventArgs)
		{
			AfterMessageOutputEventHandler afterMessageOutput = this.AfterMessageOutput;
			if (afterMessageOutput == null)
			{
				return;
			}
			afterMessageOutput(sender, messageOutputEventArgs);
		}

		// Token: 0x060026D2 RID: 9938 RVA: 0x00061410 File Offset: 0x00060410
		private void LanguageModelMgrOnAfterLocation(object sender, CompileEventArgs compileEventArgs)
		{
			CompileEventHandler afterLocation = this.AfterLocation;
			if (afterLocation == null)
			{
				return;
			}
			afterLocation(sender, compileEventArgs);
		}

		// Token: 0x060026D3 RID: 9939 RVA: 0x00061424 File Offset: 0x00060424
		private void LanguageModelMgrOnAfterGenerateGlobalInitCode(object sender, AfterGenerateGlobalInitEventArgs afterGenerateGlobalInitEventArgs)
		{
			EventHandler<AfterGenerateGlobalInitEventArgs> afterGenerateGlobalInitCode = this.AfterGenerateGlobalInitCode;
			if (afterGenerateGlobalInitCode == null)
			{
				return;
			}
			afterGenerateGlobalInitCode(sender, afterGenerateGlobalInitEventArgs);
		}

		// Token: 0x060026D4 RID: 9940 RVA: 0x00061438 File Offset: 0x00060438
		private void LanguageModelMgrOnAfterGenerateCode(object sender, CompileEventArgs compileEventArgs)
		{
			CompileEventHandler afterGenerateCode = this.AfterGenerateCode;
			if (afterGenerateCode == null)
			{
				return;
			}
			afterGenerateCode(sender, compileEventArgs);
		}

		// Token: 0x060026D5 RID: 9941 RVA: 0x0006144C File Offset: 0x0006044C
		private void LanguageModelMgrOnAfterCompile(object sender, CompileEventArgs compileEventArgs)
		{
			CompileEventHandler afterCompile = this.AfterCompile;
			if (afterCompile == null)
			{
				return;
			}
			afterCompile(sender, compileEventArgs);
		}

		// Token: 0x060026D6 RID: 9942 RVA: 0x00061460 File Offset: 0x00060460
		private void LanguageModelMgrOnAddOnlineChangeCode(object sender, AddImplicitCodeEventArgs addImplicitCodeEventArgs)
		{
			AddImplicitCodeEventHandler addOnlineChangeCode = this.AddOnlineChangeCode;
			if (addOnlineChangeCode == null)
			{
				return;
			}
			addOnlineChangeCode(sender, addImplicitCodeEventArgs);
		}

		// Token: 0x060026D7 RID: 9943 RVA: 0x00061474 File Offset: 0x00060474
		private void LanguageModelMgrOnAddLateLanguageModel(object sender, AddLanguageModelEventArgs addLanguageModelEventArgs)
		{
			AddLanguageModelEventHandler addLateLanguageModel = this.AddLateLanguageModel;
			if (addLateLanguageModel == null)
			{
				return;
			}
			addLateLanguageModel(sender, addLanguageModelEventArgs);
		}

		// Token: 0x060026D8 RID: 9944 RVA: 0x00061488 File Offset: 0x00060488
		private void LanguageModelMgrOnAddGlobalInitCode(object sender, AddImplicitCodeEventArgs addImplicitCodeEventArgs)
		{
			AddImplicitCodeEventHandler addGlobalInitCode = this.AddGlobalInitCode;
			if (addGlobalInitCode == null)
			{
				return;
			}
			addGlobalInitCode(sender, addImplicitCodeEventArgs);
		}

		// Token: 0x060026D9 RID: 9945 RVA: 0x0006149C File Offset: 0x0006049C
		private void LanguageModelMgrOnAddDownloadCode(object sender, AddImplicitCodeEventArgs addImplicitCodeEventArgs)
		{
			AddImplicitCodeEventHandler addDownloadCode = this.AddDownloadCode;
			if (addDownloadCode == null)
			{
				return;
			}
			addDownloadCode(sender, addImplicitCodeEventArgs);
		}

		// Token: 0x17000AF0 RID: 2800
		// (get) Token: 0x060026DA RID: 9946 RVA: 0x000614B0 File Offset: 0x000604B0
		public IEnumerable<ILMCompiledApplicationSet> CompiledApplicationSets
		{
			get
			{
				return APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.CompiledApplicationSets;
			}
		}

		// Token: 0x060026DB RID: 9947 RVA: 0x000614C6 File Offset: 0x000604C6
		public IApplicationContent BuildApplicationContentFromUpload(byte[] bytes)
		{
			return CompilerProxy.BuildApplicationContentFromUpload(bytes, false, true);
		}

		// Token: 0x060026DC RID: 9948 RVA: 0x000614D0 File Offset: 0x000604D0
		public IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder)
		{
			return CompilerProxy.BuildApplicationContentFromUpload(bytes, bIsMotorolaByteOrder, true);
		}

		// Token: 0x060026DD RID: 9949 RVA: 0x000614DA File Offset: 0x000604DA
		public IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder, bool bByteSupport)
		{
			return CompilerProxy.BuildApplicationContentFromUpload(bytes, bIsMotorolaByteOrder, bByteSupport);
		}

		// Token: 0x060026DE RID: 9950 RVA: 0x000614E4 File Offset: 0x000604E4
		public IAddressCalculation CreateAddressCalculaton(Guid guidApplication)
		{
			ICompileContext compileContext = APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as ICompileContext;
			if (compileContext == null)
			{
				return null;
			}
			return AddressCalculation.Create(compileContext);
		}

		// Token: 0x060026DF RID: 9951 RVA: 0x00061518 File Offset: 0x00060518
		public ulong[] GetAreaSizesAfterShrinking(Guid appGuid)
		{
			IDownloadInfo downloadInfo = this.GetDownloadInfo(appGuid, false, false);
			ulong[] array = new ulong[downloadInfo.Areas.Max((IArea a) => a.Index) + 1];
			foreach (IArea area in downloadInfo.Areas)
			{
				array[area.Index] = (ulong)((long)area.Size);
			}
			return array;
		}

		// Token: 0x060026E0 RID: 9952 RVA: 0x00061588 File Offset: 0x00060588
		public void GetCompiledIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.GetCompiledIds(guidApplication, out guidCodeId, out guidDataId);
		}

		// Token: 0x060026E1 RID: 9953 RVA: 0x0006159C File Offset: 0x0006059C
		public IDownloadInfo GetDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject)
		{
			return CompilerProxy.GetDownloadInfo(guidApplication, bOnlineChange, bBootProject, false);
		}

		// Token: 0x060026E2 RID: 9954 RVA: 0x000615A7 File Offset: 0x000605A7
		public IDownloadInfo GetRelocatedDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bOfflineBootProject, int[] nAreaMapping)
		{
			return CompilerProxy.GetRelocatedDownloadInfo(guidApplication, bOnlineChange, bBootProject, bOfflineBootProject, nAreaMapping);
		}

		// Token: 0x060026E3 RID: 9955 RVA: 0x000615B8 File Offset: 0x000605B8
		public IEnumerable<IExternalReference> GetExternalReferences(Guid guidApplication, bool bCompactDownload)
		{
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as _ICompileContext;
			if (icompileContext != null)
			{
				return CompilerProxy.GetExternalReferences(icompileContext, bCompactDownload);
			}
			return new IExternalReference[0];
		}

		// Token: 0x060026E4 RID: 9956 RVA: 0x000615F1 File Offset: 0x000605F1
		public IDownloadInfo GetOfflineBootProjectInfo(Guid guidApplication)
		{
			return CompilerProxy.GetDownloadInfo(guidApplication, false, true, true);
		}

		// Token: 0x060026E5 RID: 9957 RVA: 0x000615FC File Offset: 0x000605FC
		public IDownloadInfo GetOnlineBootProjectInfo(Guid guidApplication)
		{
			IDownloadInfo downloadInfo = CompilerProxy.GetDownloadInfo(guidApplication, false, true, false);
			_ICompileContext referenceContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(guidApplication);
			if (downloadInfo.Areas.Length != (int)referenceContext.DataManager.AreaCount)
			{
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
				{
					throw new Exception(Strings.Except_Boot_Project_Error);
				}
				foreach (IArea area in downloadInfo.Areas)
				{
					if (area is Area && (area as Area).GetAreaFlag(AreaFlags.OnlineChange))
					{
						throw new Exception(Strings.Except_Boot_Project_Error);
					}
				}
			}
			return downloadInfo;
		}

		// Token: 0x060026E6 RID: 9958 RVA: 0x00061698 File Offset: 0x00060698
		public bool IsUpToDate(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoader.CompleteLanguageModel(null);
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as _ICompileContext;
			return icompileContext != null && icompileContext.IsUpToDate();
		}

		// Token: 0x060026E7 RID: 9959 RVA: 0x000616E0 File Offset: 0x000606E0
		public bool IsUpToDate(Guid guidApplication, out bool bOnlineChangePossible)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoader.CompleteLanguageModel(null);
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as _ICompileContext;
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

		// Token: 0x060026E8 RID: 9960 RVA: 0x00061788 File Offset: 0x00060788
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
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LanguageModelMgr[applicationGuidByName];
			if (icompileContext == null)
			{
				throw new ArgumentException(Strings.ErrNoApplication);
			}
			IScope scope = CompilerProxy.CreateGlobalScope(icompileContext);
			CompilerProxy.TypifyExprement(iexpression, scope, icompileContext, null, false, false, null);
			return iexpression.GetVariable(scope);
		}

		// Token: 0x060026E9 RID: 9961 RVA: 0x000618B8 File Offset: 0x000608B8
		public Guid GetCompiledApplicationSetGuidByName(string stResourceName, string stApplicationName)
		{
			string stName = stApplicationName;
			if (!string.IsNullOrEmpty(stResourceName))
			{
				stName = stResourceName + "." + stApplicationName;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationGuidByName(stName);
		}

		// Token: 0x060026EA RID: 9962 RVA: 0x000618EC File Offset: 0x000608EC
		public ILMCompiledApplicationSet GetCompiledApplicationSet(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication);
		}

		// Token: 0x060026EB RID: 9963 RVA: 0x00061904 File Offset: 0x00060904
		public ILMCompiledApplicationTypification GetTypificator(Guid guidApplication)
		{
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as _ICompileContext;
			if (icompileContext == null)
			{
				return null;
			}
			return new CompiledApplicationTypification(icompileContext);
		}

		// Token: 0x060026EC RID: 9964 RVA: 0x00061938 File Offset: 0x00060938
		public ILMCompiledApplicationQuery QueryCompiledApplicationSet(Guid guidApplication)
		{
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as _ICompileContext;
			if (icompileContext == null)
			{
				return null;
			}
			return new CompiledApplicationQuery(icompileContext);
		}

		// Token: 0x060026ED RID: 9965 RVA: 0x0006196C File Offset: 0x0006096C
		public ILMCompiledApplicationDebugging GetApplicationDebugger(Guid guidApplication)
		{
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as _ICompileContext;
			if (icompileContext == null)
			{
				return null;
			}
			return new CompiledApplicationDebugging(icompileContext);
		}

		// Token: 0x060026EE RID: 9966 RVA: 0x000619A0 File Offset: 0x000609A0
		public ILMCompiledApplicationContentDumper GetCompiledApplicationContentDumper(Guid guidApplication)
		{
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as _ICompileContext;
			if (icompileContext == null)
			{
				return null;
			}
			return new CompiledApplicationContentDumper(icompileContext);
		}

		// Token: 0x060026EF RID: 9967 RVA: 0x000619D3 File Offset: 0x000609D3
		public IType GetTypeOfLiteral(ILiteralExpression litExp, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32, bool bUnicodeNotSupported)
		{
			if (litExp == null)
			{
				throw new ArgumentNullException("litExp");
			}
			return CompilerProxy.GetTypeOfLiteral((_ILiteralExpression)litExp, bLRealSupported, bTreatLRealAsReal, bInt64Supported, bTreatInt64AsInt32, bUnicodeNotSupported);
		}

		// Token: 0x060026F0 RID: 9968 RVA: 0x000619F6 File Offset: 0x000609F6
		public IType GetTypeOfLiteral(ILiteralExpression litExp, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32)
		{
			if (litExp == null)
			{
				throw new ArgumentNullException("litExp");
			}
			return CompilerProxy.GetTypeOfLiteral((_ILiteralExpression)litExp, bLRealSupported, bTreatLRealAsReal, bInt64Supported, bTreatInt64AsInt32, true);
		}

		// Token: 0x060026F1 RID: 9969 RVA: 0x00061A18 File Offset: 0x00060A18
		public IEnumerable<ITaskCrossref> GetTaskReferencesOfInstancePath(string stInstancePath)
		{
			if (stInstancePath == null)
			{
				throw new ArgumentNullException("stInstancePath");
			}
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stInstancePath, false, false, false, false);
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
			_IExpression iexpression = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner) as _IParser).ParseSTOperand(out flag);
			if (flag)
			{
				throw new ArgumentException(Strings.ErrInvalidExpression);
			}
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LanguageModelMgr[applicationGuidByName];
			if (icompileContext == null)
			{
				throw new ArgumentException(Strings.ErrNoApplication);
			}
			IScope scope = CompilerProxy.CreateGlobalScope(icompileContext);
			CompilerProxy.TypifyExprement(iexpression, scope, icompileContext, null, false, false, null);
			bool variable = iexpression.GetVariable(scope) != null;
			iexpression.GetSignatureEx(scope);
			ISignature signature = scope[iexpression.SignatureId];
			if (!variable || signature == null)
			{
				return new LList<ITaskCrossref>(0);
			}
			return CompileService.GetTaskReferences(iexpression, scope, icompileContext);
		}

		// Token: 0x060026F2 RID: 9970 RVA: 0x00061B88 File Offset: 0x00060B88
		private static LList<ITaskCrossref> GetTaskReferences(_IExpression exp, IScope scope, _ICompileContext comcon)
		{
			_IVariable ivariable = exp.GetVariable(scope) as _IVariable;
			ISignature signature = scope[exp.SignatureId];
			if (ivariable == null || signature == null)
			{
				return null;
			}
			LList<ITaskCrossref> llist = new LList<ITaskCrossref>();
			llist.AddRange(CompilerProxy.GetTaskRefIds(ivariable, signature, false, false, comcon));
			if (exp is _ICompoAccessExpression)
			{
				LList<ITaskCrossref> taskReferences = CompileService.GetTaskReferences((exp as _ICompoAccessExpression)._Left, scope, comcon);
				if (taskReferences != null)
				{
					LList<ITaskCrossref> llist2 = llist;
					llist = new LList<ITaskCrossref>();
					foreach (ITaskCrossref taskCrossref in taskReferences)
					{
						foreach (ITaskCrossref taskCrossref2 in llist2)
						{
							if (taskCrossref.TaskId == taskCrossref2.TaskId)
							{
								llist.Add(taskCrossref2);
								break;
							}
						}
					}
				}
			}
			return llist;
		}

		// Token: 0x060026F3 RID: 9971 RVA: 0x00061C84 File Offset: 0x00060C84
		public string GetCompilerDefinesOfDeviceDescription(Guid guidApplication)
		{
			string result = string.Empty;
			Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(guidApplication);
			if (Guid.Empty != deviceOfApplication)
			{
				IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(deviceOfApplication);
				if (targetIdOfDevice != null)
				{
					ITargetSettings targetSettingsById = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
					if (targetSettingsById != null)
					{
						result = LocalTargetSettings.CompilerDefines.GetStringValue(targetSettingsById);
					}
				}
			}
			return result;
		}

		// Token: 0x060026F4 RID: 9972 RVA: 0x00061CEF File Offset: 0x00060CEF
		public virtual void OnAfterUnsuccessfullGenerateCode(AfterUnsuccessfullGenerateCodeEventArgs e)
		{
			AfterUnsuccessfullGenerateCodeEventHandler afterUnsuccessfullGenerateCode = this.AfterUnsuccessfullGenerateCode;
			if (afterUnsuccessfullGenerateCode == null)
			{
				return;
			}
			afterUnsuccessfullGenerateCode(this, e);
		}

		// Token: 0x0400075E RID: 1886
		private readonly LanguageModelManagerConsolidated _lmm;
	}
}
