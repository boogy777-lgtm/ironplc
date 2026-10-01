using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x0200024D RID: 589
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "There seems to be an error in counting classes: the event itself is counted as class, but should be counted as a method. Anyway, splitting this class does not increase readability")]
	public class PreCompileService : ILMPreCompileService5, ILMPreCompileService4, ILMPreCompileService3, ILMPreCompileService2, ILMPreCompileService, ILMPreCompileCheckerService2, ILMPreCompileCheckerService
	{
		// Token: 0x1400004D RID: 77
		// (add) Token: 0x0600273B RID: 10043 RVA: 0x00062878 File Offset: 0x00061878
		// (remove) Token: 0x0600273C RID: 10044 RVA: 0x000628B0 File Offset: 0x000618B0
		public event SimulationModeChangedEventHandler AfterSimulationModeChanged;

		// Token: 0x1400004E RID: 78
		// (add) Token: 0x0600273D RID: 10045 RVA: 0x000628E8 File Offset: 0x000618E8
		// (remove) Token: 0x0600273E RID: 10046 RVA: 0x00062920 File Offset: 0x00061920
		public event CompiledPOUChangedEventHandler CompiledPOUChanged;

		// Token: 0x1400004F RID: 79
		// (add) Token: 0x0600273F RID: 10047 RVA: 0x00062958 File Offset: 0x00061958
		// (remove) Token: 0x06002740 RID: 10048 RVA: 0x00062990 File Offset: 0x00061990
		public event CompiledPOUChangedEventHandler CompiledPOUDeleted;

		// Token: 0x14000050 RID: 80
		// (add) Token: 0x06002741 RID: 10049 RVA: 0x000629C8 File Offset: 0x000619C8
		// (remove) Token: 0x06002742 RID: 10050 RVA: 0x00062A00 File Offset: 0x00061A00
		public event CompiledPOUChangedEventHandler CompiledPOUInserted;

		// Token: 0x14000051 RID: 81
		// (add) Token: 0x06002743 RID: 10051 RVA: 0x00062A38 File Offset: 0x00061A38
		// (remove) Token: 0x06002744 RID: 10052 RVA: 0x00062A70 File Offset: 0x00061A70
		public event IsHiddenVariableEventHandler IsHiddenVariableHandler;

		// Token: 0x14000052 RID: 82
		// (add) Token: 0x06002745 RID: 10053 RVA: 0x00062AA8 File Offset: 0x00061AA8
		// (remove) Token: 0x06002746 RID: 10054 RVA: 0x00062AE0 File Offset: 0x00061AE0
		public event EventHandler<LibraryContextDeletedEventArgs> LibrarySetDeleted;

		// Token: 0x14000053 RID: 83
		// (add) Token: 0x06002747 RID: 10055 RVA: 0x00062B18 File Offset: 0x00061B18
		// (remove) Token: 0x06002748 RID: 10056 RVA: 0x00062B50 File Offset: 0x00061B50
		public event SignatureChangedEventHandler SignatureChanged;

		// Token: 0x14000054 RID: 84
		// (add) Token: 0x06002749 RID: 10057 RVA: 0x00062B88 File Offset: 0x00061B88
		// (remove) Token: 0x0600274A RID: 10058 RVA: 0x00062BC0 File Offset: 0x00061BC0
		public event SignatureChangedEventHandler SignatureDeleted;

		// Token: 0x14000055 RID: 85
		// (add) Token: 0x0600274B RID: 10059 RVA: 0x00062BF8 File Offset: 0x00061BF8
		// (remove) Token: 0x0600274C RID: 10060 RVA: 0x00062C30 File Offset: 0x00061C30
		public event SignatureChangedEventHandler SignatureInserted;

		// Token: 0x14000056 RID: 86
		// (add) Token: 0x0600274D RID: 10061 RVA: 0x00062C68 File Offset: 0x00061C68
		// (remove) Token: 0x0600274E RID: 10062 RVA: 0x00062CA0 File Offset: 0x00061CA0
		public event CompileEventHandler TaskConfigChanged;

		// Token: 0x14000057 RID: 87
		// (add) Token: 0x0600274F RID: 10063 RVA: 0x00062CD8 File Offset: 0x00061CD8
		// (remove) Token: 0x06002750 RID: 10064 RVA: 0x00062D10 File Offset: 0x00061D10
		public event CompileEventHandler AfterPrecompileChecksDone;

		// Token: 0x06002751 RID: 10065 RVA: 0x00062D48 File Offset: 0x00061D48
		public PreCompileService(LanguageModelManagerConsolidated lmm)
		{
			lmm.AfterSimulationModeChanged += this.LanguageModelMgrOnAfterSimulationModeChanged;
			lmm.CompiledPOUChanged += this.LanguageModelMgrOnCompiledPouChanged;
			lmm.CompiledPOUDeleted += this.LanguageModelMgrOnCompiledPouDeleted;
			lmm.CompiledPOUInserted += this.LanguageModelMgrOnCompiledPouInserted;
			lmm.IsHiddenVariableHandler += this.LanguageModelMgrOnIsHiddenVariableHandler;
			lmm.LibraryContextDeleted += this.LanguageModelMgrOnLibraryContextDeleted;
			lmm.SignatureChanged += this.LanguageModelMgrOnSignatureChanged;
			lmm.SignatureDeleted += this.LanguageModelMgrOnSignatureDeleted;
			lmm.SignatureInserted += this.LanguageModelMgrOnSignatureInserted;
			lmm.TaskConfigChanged += this.LanguageModelMgrOnTaskConfigChanged;
		}

		// Token: 0x06002752 RID: 10066 RVA: 0x00062E1A File Offset: 0x00061E1A
		public void OnAllSystemInstancesAvailable()
		{
			APEnvironmentFacade.Instance.PrimaryProjectSwitched += this.ProjectsOnPrimaryProjectSwitched;
			this.RegisterPrecompilePreCondition(new ProjectLoadedPrecompileCondition(APEnvironmentFacade.Instance.LanguageModelMgr));
		}

		// Token: 0x17000AF8 RID: 2808
		// (get) Token: 0x06002753 RID: 10067 RVA: 0x00062E48 File Offset: 0x00061E48
		private List<IPrecompilePrecondition> PrecompilePreconditions { get; } = new List<IPrecompilePrecondition>();

		// Token: 0x06002754 RID: 10068 RVA: 0x00062E50 File Offset: 0x00061E50
		private void ProjectsOnPrimaryProjectSwitched(IProject oldproject, IProject newproject)
		{
			if (this.PrecompilePreconditions.All((IPrecompilePrecondition p) => p.IsDone))
			{
				CompilerProxy.GetCheckerThread().Enable();
				return;
			}
			CompilerProxy.GetCheckerThread().Disable();
		}

		// Token: 0x06002755 RID: 10069 RVA: 0x00062E9E File Offset: 0x00061E9E
		private void PrecompileIsReady(object sender, EventArgs e)
		{
			if (this.PrecompilePreconditions.All((IPrecompilePrecondition p) => p.IsDone))
			{
				CompilerProxy.GetCheckerThread().Enable();
			}
		}

		// Token: 0x06002756 RID: 10070 RVA: 0x00062ED6 File Offset: 0x00061ED6
		public IDisposable RegisterPrecompilePreCondition(IPrecompilePrecondition precondition)
		{
			return new PreCompileService.PreconditionDisposer(precondition, this);
		}

		// Token: 0x17000AF9 RID: 2809
		// (get) Token: 0x06002757 RID: 10071 RVA: 0x00062EDF File Offset: 0x00061EDF
		// (set) Token: 0x06002758 RID: 10072 RVA: 0x00062EE7 File Offset: 0x00061EE7
		public bool PrecompileChecksDone
		{
			get
			{
				return this._bPrecompileChecksDone;
			}
			set
			{
				if (!this._bPrecompileChecksDone && value)
				{
					this._bPrecompileChecksDone = value;
					this.OnAfterPrecompileChecksDone(this, new CompileEventArgs(APEnvironmentFacade.Instance.ActiveApplicationGuid));
				}
				this._bPrecompileChecksDone = value;
			}
		}

		// Token: 0x06002759 RID: 10073 RVA: 0x0006274A File Offset: 0x0006174A
		public void FinishPrecompileChecks()
		{
			CompilerProxy.GetCheckerThread().FinishPrecompileChecks();
		}

		// Token: 0x0600275A RID: 10074 RVA: 0x00062F1A File Offset: 0x00061F1A
		private void OnAfterPrecompileChecksDone(object sender, CompileEventArgs e)
		{
			CompileEventHandler afterPrecompileChecksDone = this.AfterPrecompileChecksDone;
			if (afterPrecompileChecksDone == null)
			{
				return;
			}
			afterPrecompileChecksDone(sender, e);
		}

		// Token: 0x0600275B RID: 10075 RVA: 0x00062F2E File Offset: 0x00061F2E
		private void LanguageModelMgrOnTaskConfigChanged(object sender, CompileEventArgs compileEventArgs)
		{
			CompileEventHandler taskConfigChanged = this.TaskConfigChanged;
			if (taskConfigChanged == null)
			{
				return;
			}
			taskConfigChanged(sender, compileEventArgs);
		}

		// Token: 0x0600275C RID: 10076 RVA: 0x00062F42 File Offset: 0x00061F42
		private void LanguageModelMgrOnSignatureInserted(object sender, SignatureChangedEventArgs signatureChangedEventArgs)
		{
			SignatureChangedEventHandler signatureInserted = this.SignatureInserted;
			if (signatureInserted == null)
			{
				return;
			}
			signatureInserted(sender, signatureChangedEventArgs);
		}

		// Token: 0x0600275D RID: 10077 RVA: 0x00062F56 File Offset: 0x00061F56
		private void LanguageModelMgrOnSignatureDeleted(object sender, SignatureChangedEventArgs signatureChangedEventArgs)
		{
			SignatureChangedEventHandler signatureDeleted = this.SignatureDeleted;
			if (signatureDeleted == null)
			{
				return;
			}
			signatureDeleted(sender, signatureChangedEventArgs);
		}

		// Token: 0x0600275E RID: 10078 RVA: 0x00062F6A File Offset: 0x00061F6A
		private void LanguageModelMgrOnSignatureChanged(object sender, SignatureChangedEventArgs signatureChangedEventArgs)
		{
			SignatureChangedEventHandler signatureChanged = this.SignatureChanged;
			if (signatureChanged == null)
			{
				return;
			}
			signatureChanged(sender, signatureChangedEventArgs);
		}

		// Token: 0x0600275F RID: 10079 RVA: 0x00062F7E File Offset: 0x00061F7E
		private void LanguageModelMgrOnLibraryContextDeleted(object sender, LibraryContextDeletedEventArgs libraryContextDeletedEventArgs)
		{
			EventHandler<LibraryContextDeletedEventArgs> librarySetDeleted = this.LibrarySetDeleted;
			if (librarySetDeleted == null)
			{
				return;
			}
			librarySetDeleted(sender, libraryContextDeletedEventArgs);
		}

		// Token: 0x06002760 RID: 10080 RVA: 0x00062F92 File Offset: 0x00061F92
		private void LanguageModelMgrOnIsHiddenVariableHandler(object sender, IsHiddenVariableEventArgs args)
		{
			IsHiddenVariableEventHandler isHiddenVariableHandler = this.IsHiddenVariableHandler;
			if (isHiddenVariableHandler == null)
			{
				return;
			}
			isHiddenVariableHandler(sender, args);
		}

		// Token: 0x06002761 RID: 10081 RVA: 0x00062FA6 File Offset: 0x00061FA6
		private void LanguageModelMgrOnCompiledPouInserted(object sender, CompiledPOUChangedEventArgs compiledPouChangedEventArgs)
		{
			CompiledPOUChangedEventHandler compiledPOUInserted = this.CompiledPOUInserted;
			if (compiledPOUInserted == null)
			{
				return;
			}
			compiledPOUInserted(sender, compiledPouChangedEventArgs);
		}

		// Token: 0x06002762 RID: 10082 RVA: 0x00062FBA File Offset: 0x00061FBA
		private void LanguageModelMgrOnCompiledPouDeleted(object sender, CompiledPOUChangedEventArgs compiledPouChangedEventArgs)
		{
			CompiledPOUChangedEventHandler compiledPOUDeleted = this.CompiledPOUDeleted;
			if (compiledPOUDeleted == null)
			{
				return;
			}
			compiledPOUDeleted(sender, compiledPouChangedEventArgs);
		}

		// Token: 0x06002763 RID: 10083 RVA: 0x00062FCE File Offset: 0x00061FCE
		private void LanguageModelMgrOnCompiledPouChanged(object sender, CompiledPOUChangedEventArgs compiledPouChangedEventArgs)
		{
			CompiledPOUChangedEventHandler compiledPOUChanged = this.CompiledPOUChanged;
			if (compiledPOUChanged == null)
			{
				return;
			}
			compiledPOUChanged(sender, compiledPouChangedEventArgs);
		}

		// Token: 0x06002764 RID: 10084 RVA: 0x00062FE2 File Offset: 0x00061FE2
		private void LanguageModelMgrOnAfterSimulationModeChanged(object sender, SimulationModeArgs args)
		{
			SimulationModeChangedEventHandler afterSimulationModeChanged = this.AfterSimulationModeChanged;
			if (afterSimulationModeChanged == null)
			{
				return;
			}
			afterSimulationModeChanged(sender, args);
		}

		// Token: 0x17000AFA RID: 2810
		// (get) Token: 0x06002765 RID: 10085 RVA: 0x00062FF6 File Offset: 0x00061FF6
		public IEnumerable<ILMPreCompileSet> LibrarySets
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.LibraryContexts.Cast<ILMPreCompileSet>();
			}
		}

		// Token: 0x17000AFB RID: 2811
		// (get) Token: 0x06002766 RID: 10086 RVA: 0x0006300C File Offset: 0x0006200C
		public IEnumerable<ILMPreCompileSet> PrecompileSets
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.PrecompileContexts.Cast<ILMPreCompileSet>();
			}
		}

		// Token: 0x17000AFC RID: 2812
		// (get) Token: 0x06002767 RID: 10087 RVA: 0x00063022 File Offset: 0x00062022
		public ILMPreCompileSet SystemSet
		{
			get
			{
				return (ILMPreCompileSet)APEnvironmentFacade.Instance.LanguageModelMgr.SystemContext;
			}
		}

		// Token: 0x06002768 RID: 10088 RVA: 0x00063038 File Offset: 0x00062038
		public IEnumerable<ILMPreCompileSet> AllPreCompileSets(bool bWithDevices, bool bWithLibraries)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr._AllPreCompileContexts(bWithDevices, bWithLibraries).Cast<ILMPreCompileSet>();
		}

		// Token: 0x06002769 RID: 10089 RVA: 0x00063050 File Offset: 0x00062050
		public IEnumerable<ISignature> AllPrecompiledSignatures(bool bWithLibraries, bool bWithResources)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.AllPrecompiledSignatures(bWithLibraries, bWithResources);
		}

		// Token: 0x0600276A RID: 10090 RVA: 0x00063064 File Offset: 0x00062064
		public bool ExpressionsEqual(IExprement exp1, IExprement exp2)
		{
			ExpressionComparer expressionComparer = new ExpressionComparer(exp2 as _IExprement);
			(exp1 as _IExprement).Accept(expressionComparer);
			return expressionComparer.CodeEqual;
		}

		// Token: 0x0600276B RID: 10091 RVA: 0x00063090 File Offset: 0x00062090
		public ISignature FindSignature(Guid guidObject, out ILMPreCompileSet precom)
		{
			IPreCompileContext preCompileContext;
			ISignature result = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(guidObject, out preCompileContext);
			precom = (ILMPreCompileSet)preCompileContext;
			return result;
		}

		// Token: 0x0600276C RID: 10092 RVA: 0x000630B7 File Offset: 0x000620B7
		public ISignature FindSignature(int nProjectHandle, Guid guidObject, out IPreCompileContext preCompileContext)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(nProjectHandle, guidObject, out preCompileContext);
		}

		// Token: 0x0600276D RID: 10093 RVA: 0x000630CB File Offset: 0x000620CB
		public IEnumerable<ISignature> FindSignaturesByName(int nProjectHandle, Guid callingObjectGuid, string stName)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.FindSignaturesByName(nProjectHandle, callingObjectGuid, stName);
		}

		// Token: 0x0600276E RID: 10094 RVA: 0x000630DF File Offset: 0x000620DF
		public IEnumerable<ISignature> FindSignaturesByName(int nProjectHandle, Guid applicationGuid, Guid callingObjectGuid, string stName)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.FindSignaturesByName(nProjectHandle, applicationGuid, callingObjectGuid, stName);
		}

		// Token: 0x0600276F RID: 10095 RVA: 0x000630F5 File Offset: 0x000620F5
		public IEnumerable<ISignature2> GetAllInterfaces(ISignature2 sign)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetAllInterfaces(sign);
		}

		// Token: 0x06002770 RID: 10096 RVA: 0x00063107 File Offset: 0x00062107
		public IEnumerable<ISignature2> GetAllInterfaces(ISignature2 sign, Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetAllInterfaces(sign, guidApplication);
		}

		// Token: 0x06002771 RID: 10097 RVA: 0x0006311A File Offset: 0x0006211A
		public IEnumerable<ISignature2> GetAllMethods(ISignature2 sign)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetAllMethods(sign);
		}

		// Token: 0x06002772 RID: 10098 RVA: 0x0006312C File Offset: 0x0006212C
		public IEnumerable<ISignature2> GetAllMethods(ISignature2 sign, Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetAllMethods(sign, guidApplication);
		}

		// Token: 0x06002773 RID: 10099 RVA: 0x0006313F File Offset: 0x0006213F
		public IEnumerable<IVariable2> GetAllVariables(ISignature2 sign)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetAllVariables(sign);
		}

		// Token: 0x06002774 RID: 10100 RVA: 0x00063151 File Offset: 0x00062151
		public IEnumerable<IVariable2> GetAllVariables(ISignature2 sign, Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetAllVariables(sign, guidApplication);
		}

		// Token: 0x06002775 RID: 10101 RVA: 0x00063164 File Offset: 0x00062164
		public ISignature2 GetBaseSignature(ISignature2 sign)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetBaseSignature(sign);
		}

		// Token: 0x06002776 RID: 10102 RVA: 0x00063176 File Offset: 0x00062176
		public ISignature2 GetBaseSignature(ISignature2 sign, Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetBaseSignature(sign, guidApplication);
		}

		// Token: 0x06002777 RID: 10103 RVA: 0x00063189 File Offset: 0x00062189
		public IEnumerable<ISignature2> GetInterfaceSignatures(ISignature2 sign)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetInterfaceSignatures(sign);
		}

		// Token: 0x06002778 RID: 10104 RVA: 0x0006319B File Offset: 0x0006219B
		public IEnumerable<ISignature2> GetInterfaceSignatures(ISignature2 sign, Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetInterfaceSignatures(sign, guidApplication);
		}

		// Token: 0x06002779 RID: 10105 RVA: 0x000631AE File Offset: 0x000621AE
		public ILMPreCompileSet GetLibraryPrecompileSet(string stLibraryId)
		{
			return (ILMPreCompileSet)APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryPrecompileContext(stLibraryId);
		}

		// Token: 0x0600277A RID: 10106 RVA: 0x000631C5 File Offset: 0x000621C5
		public ILMPreCompileSet GetPrecompileSetOfSignature(ISignature sign)
		{
			return (ILMPreCompileSet)APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(sign);
		}

		// Token: 0x0600277B RID: 10107 RVA: 0x000631DC File Offset: 0x000621DC
		public ISignature6 GetSignatureForPrecompileID(int precompileId)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(precompileId);
		}

		// Token: 0x0600277C RID: 10108 RVA: 0x000631EE File Offset: 0x000621EE
		public bool IsHiddenSignature(ISignature signature, GUIHidingFlags flagsToConsider)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(signature, flagsToConsider);
		}

		// Token: 0x0600277D RID: 10109 RVA: 0x00063201 File Offset: 0x00062201
		public bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(signature, variable, flagsToConsider);
		}

		// Token: 0x0600277E RID: 10110 RVA: 0x00063215 File Offset: 0x00062215
		public bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider, ISignature signCurrent)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(signature, variable, flagsToConsider, signCurrent);
		}

		// Token: 0x0600277F RID: 10111 RVA: 0x0006322C File Offset: 0x0006222C
		public IPrecompileScope CreatePrecompileScope(ILMPreCompileSet precompileSet, Guid guidSignature)
		{
			_ISignature signature = ((_IPreCompileContext)precompileSet)[guidSignature];
			return CompilerProxy.CreatePrecompileScope((_IPreCompileContext)precompileSet, signature);
		}

		// Token: 0x06002780 RID: 10112 RVA: 0x00063252 File Offset: 0x00062252
		public ILMPreCompileSet GetPreCompileSet(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guidApplication) as ILMPreCompileSet;
		}

		// Token: 0x06002781 RID: 10113 RVA: 0x0006326C File Offset: 0x0006226C
		public ISignature FindSignature(ELMPreCompileSetType ePreCompileSetTypesToConsider, Guid guidObject, out ILMPreCompileSet precom)
		{
			IPreCompileContext preCompileContext = null;
			ISignature result = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(ePreCompileSetTypesToConsider, guidObject, out preCompileContext);
			precom = (ILMPreCompileSet)preCompileContext;
			return result;
		}

		// Token: 0x06002782 RID: 10114 RVA: 0x00063296 File Offset: 0x00062296
		public ILMPreCompileTypifier CreatePreCompileTypifier(Guid guidApplication)
		{
			return CompilerProxy.CreatePreCompileTypifier(guidApplication);
		}

		// Token: 0x06002783 RID: 10115 RVA: 0x000632A0 File Offset: 0x000622A0
		public IEnumerable<IMessage> GetExprementMessages(IExprement exprement)
		{
			_IExprement iexprement = exprement as _IExprement;
			if (iexprement == null)
			{
				return new IMessage[0];
			}
			IMessage[] exprementMessages = CompilerProxy.GetExprementMessages(iexprement);
			if (exprementMessages == null)
			{
				return new IMessage[0];
			}
			return exprementMessages;
		}

		// Token: 0x06002784 RID: 10116 RVA: 0x000632D0 File Offset: 0x000622D0
		public ILMQualifierService CreateQualifierService()
		{
			return CompilerProxy._QualifierService_OrNull;
		}

		// Token: 0x06002785 RID: 10117 RVA: 0x000632D8 File Offset: 0x000622D8
		public ILiteralValue GetEnumInitValue(IVariable variable, ICommonScope scope)
		{
			IEnumType enumType = variable.Type as IEnumType;
			if (enumType == null)
			{
				throw new ArgumentException("The variable is no enum member. Expected a variable, that represents a value like \"Months.January\", where month is a ENUM and January its member");
			}
			IList<_IVariable> allVariables = ((_ISignature)scope.FindSignature(enumType)).AllVariables;
			if (allVariables == null || allVariables.Count == 0)
			{
				return null;
			}
			long num = 0L;
			foreach (_IVariable ivariable in allVariables)
			{
				if (ivariable.Initial != null)
				{
					ILiteralValue literalValue = scope.GetLiteralValue(ivariable.Initial, true);
					long num2;
					if (literalValue == null || !literalValue.GetSignedLong(out num2))
					{
						return null;
					}
					num = literalValue.SignedLong;
				}
				if (ivariable.Name == variable.Name)
				{
					break;
				}
				num += 1L;
			}
			return LanguageModelBuilder.Singleton.CreateLiteralValue(num);
		}

		// Token: 0x06002786 RID: 10118 RVA: 0x000633B4 File Offset: 0x000623B4
		public ILMTypeService CreateTypeService()
		{
			return CompilerProxy._TypeService_OrNull;
		}

		// Token: 0x06002787 RID: 10119 RVA: 0x000633BB File Offset: 0x000623BB
		public ILMStringEncodingService CreateStringEncodingService()
		{
			return CompilerProxy._StringEncodingService_OrNull;
		}

		// Token: 0x0400076B RID: 1899
		private bool _bPrecompileChecksDone;

		// Token: 0x020002D6 RID: 726
		private sealed class PreconditionDisposer : IDisposable
		{
			// Token: 0x06002C7E RID: 11390 RVA: 0x00074C89 File Offset: 0x00073C89
			public PreconditionDisposer(IPrecompilePrecondition value, PreCompileService self)
			{
				this.Value = value;
				this.Self = self;
				this.Self.PrecompilePreconditions.Add(value);
				value.WhenDone += this.Self.PrecompileIsReady;
			}

			// Token: 0x06002C7F RID: 11391 RVA: 0x00074CC8 File Offset: 0x00073CC8
			public void Dispose()
			{
				if (this.Self == null)
				{
					return;
				}
				this.Self.PrecompilePreconditions.Remove(this.Value);
				this.Value.WhenDone -= this.Self.PrecompileIsReady;
				this.Self = null;
			}

			// Token: 0x04000901 RID: 2305
			private readonly IPrecompilePrecondition Value;

			// Token: 0x04000902 RID: 2306
			private PreCompileService Self;
		}
	}
}
