using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManagerLegacy
{
	[TypeGuid("{AABD63B4-12B8-436d-B7DC-4EE37ED323CB}")]
	[SystemInterface("_3S.CoDeSys.Core.LanguageModel.ILanguageModelManager")]
	public class LanguageModelManagerLegacy : ILanguageModelManager20, ILanguageModelManager19, ILanguageModelManager18, ILanguageModelManager17, ILanguageModelManager16, ILanguageModelManager15, ILanguageModelManager14, ILanguageModelManager13, ILanguageModelManager12, ILanguageModelManager11, ILanguageModelManager10, ILanguageModelManager9, ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager, ILanguageModelManager30, ILanguageModelManager29, ILanguageModelManager28, ILanguageModelManager27, ILanguageModelManager26, ILanguageModelManager25, ILanguageModelManager24, ILanguageModelManager23, ILanguageModelManager22, ILanguageModelManager21, ISystemInstanceRequiresInitialization
	{
		private ILMServiceProvider _lmServiceProvider;

		private ILMServiceProvider LMServiceProvider => _lmServiceProvider ?? APEnvironment.LMServiceProvider;

		public bool DelayedLoaderWorking => LMServiceProvider.LanguageModelProviderService.DelayedLoaderWorking;

		public int PendingLibrariesInLoadQueue => LMServiceProvider.LanguageModelProviderService.PendingLibrariesInLoadQueue;

		public IPreCompileContext[] LibraryContexts => LMServiceProvider.PreCompileService.LibrarySets.Cast<IPreCompileContext>().ToArray();

		public ICompileContext[] CompileContexts => LMServiceProvider.CompileService.CompiledApplicationSets.Cast<ICompileContext>().ToArray();

		public IPreCompileContext[] PrecompileContexts => LMServiceProvider.PreCompileService.PrecompileSets.Cast<IPreCompileContext>().ToArray();

		public ICompileContext[] ReferenceContexts => LMServiceProvider.DownloadedApplicationService.CompiledApplicationSets.Cast<ICompileContext>().ToArray();

		public bool PrecompileInformationUpToDate => LMServiceProvider.PreCompileCrossReferenceService.PrecompileInformationUpToDate;

		public IEnumerable<IAttribute> RegisteredAttributes => LMServiceProvider.ConfigurationService.RegisteredAttributes;

		public IPreCompileContext SystemContext => LMServiceProvider.PreCompileService.SystemSet as IPreCompileContext;

		public ITypeInfo TypeInfo => LMServiceProvider.CreatorService.TypeInfo;

		public event AddImplicitCodeEventHandler AddDownloadCode;

		public event AddImplicitCodeEventHandler AddGlobalInitCode;

		public event AddLanguageModelEventHandler AddLateLanguageModel;

		public event AddImplicitCodeEventHandler AddOnlineChangeCode;

		public event EventHandler AfterClearAll;

		public event CompileEventHandler AfterCompile;

		public event CompileEventHandler AfterGenerateCode;

		public event EventHandler<AfterGenerateGlobalInitEventArgs> AfterGenerateGlobalInitCode;

		public event EventHandler AfterLazyLibraryLoad;

		public event CompileEventHandler AfterLocation;

		public event AfterMessageOutputEventHandler AfterMessageOutput;

		public event SimulationModeChangedEventHandler AfterSimulationModeChanged;

		public event EventHandler BeforeClearAll;

		public event CompileEventHandler BeforeCompile
		{
			add
			{
				LMServiceProvider.CompileService.BeforeCompile += value;
			}
			remove
			{
				LMServiceProvider.CompileService.BeforeCompile -= value;
			}
		}

		public event EventHandler<CompileEventArgs> BeforeGenerateCompiledCode;

		public event EventHandler<BeforeGenerateRelinkCodeEventArgs> BeforeGenerateRelinkCode;

		public event CompileEventHandler BeforeLocation;

		public event BeforeMessageOutputEventHandler BeforeMessageOutput;

		public event CompileEventHandler CodeChanged;

		public event CompiledPOUChangedEventHandler CompiledPOUChanged;

		public event CompiledPOUChangedEventHandler CompiledPOUDeleted;

		public event CompiledPOUChangedEventHandler CompiledPOUInserted;

		public event FilterMessageOutputEventHandler FilterMessageOutput;

		public event IsHiddenVariableEventHandler IsHiddenVariableHandler;

		public event EventHandler<LibraryContextDeletedEventArgs> LibraryContextDeleted;

		public event SetLibraryPreCompileContextCompletionPostProcessEventHandler SetLibraryPreCompileContextCompletionPostProcess;

		public event SignatureChangedEventHandler SignatureChanged;

		public event SignatureChangedEventHandler SignatureDeleted;

		public event SignatureChangedEventHandler SignatureInserted;

		public event CompileEventHandler TaskConfigChanged;

		public LanguageModelManagerLegacy()
		{
		}

		public LanguageModelManagerLegacy(ILMServiceProvider lmServiceProvider)
		{
			_lmServiceProvider = lmServiceProvider;
			Initialize();
		}

		public void OnAllSystemInstancesAvailable()
		{
			_lmServiceProvider = APEnvironment.LMServiceProvider;
			Initialize();
		}

		public void Initialize()
		{
			LMServiceProvider.CompileService.AddDownloadCode += LanguageModelMgrConsolidated_AddDownloadCode;
			LMServiceProvider.CompileService.AddGlobalInitCode += LanguageModelMgrConsolidated_AddGlobalInitCode;
			LMServiceProvider.CompileService.AddLateLanguageModel += LanguageModelMgrConsolidated_AddLateLanguageModel;
			LMServiceProvider.CompileService.AddOnlineChangeCode += LanguageModelMgrConsolidated_AddOnlineChangeCode;
			LMServiceProvider.CommandService.AfterClearAll += LanguageModelMgrConsolidated_AfterClearAll;
			LMServiceProvider.CompileService.AfterCompile += LanguageModelMgrConsolidated_AfterCompile;
			LMServiceProvider.CompileService.AfterGenerateCode += LanguageModelMgrConsolidated_AfterGenerateCode;
			LMServiceProvider.CompileService.AfterGenerateGlobalInitCode += LanguageModelMgrConsolidated_AfterGenerateGlobalInitCode;
			LMServiceProvider.LanguageModelProviderService.AfterLazyLibraryLoad += LanguageModelMgrConsolidated_AfterLazyLibraryLoad;
			LMServiceProvider.CompileService.AfterLocation += LanguageModelMgrConsolidated_AfterLocation;
			LMServiceProvider.CompileService.AfterMessageOutput += LanguageModelMgrConsolidated_AfterMessageOutput;
			LMServiceProvider.PreCompileService.AfterSimulationModeChanged += LanguageModelMgrConsolidated_AfterSimulationModeChanged;
			LMServiceProvider.CommandService.BeforeClearAll += LanguageModelMgrConsolidated_BeforeClearAll;
			LMServiceProvider.CompileService.BeforeGenerateCompiledCode += LanguageModelMgrConsolidated_BeforeGenerateCompiledCode;
			LMServiceProvider.CompileService.BeforeGenerateRelinkCode += LanguageModelMgrConsolidated_BeforeGenerateRelinkCode;
			LMServiceProvider.CompileService.BeforeLocation += LanguageModelMgrConsolidated_BeforeLocation;
			LMServiceProvider.CompileService.BeforeMessageOutput += LanguageModelMgrConsolidated_BeforeMessageOutput;
			LMServiceProvider.CompileService.CodeChanged += LanguageModelMgrConsolidated_CodeChanged;
			LMServiceProvider.PreCompileService.CompiledPOUChanged += LanguageModelMgrConsolidated_CompiledPOUChanged;
			LMServiceProvider.PreCompileService.CompiledPOUDeleted += LanguageModelMgrConsolidated_CompiledPOUDeleted;
			LMServiceProvider.PreCompileService.CompiledPOUInserted += LanguageModelMgrConsolidated_CompiledPOUInserted;
			LMServiceProvider.CompileService.FilterMessageOutput += LanguageModelMgrConsolidated_FilterMessageOutput;
			LMServiceProvider.PreCompileService.IsHiddenVariableHandler += LanguageModelMgrConsolidated_IsHiddenVariableHandler;
			LMServiceProvider.PreCompileService.LibrarySetDeleted += LanguageModelMgrConsolidated_LibraryContextDeleted;
			LMServiceProvider.LanguageModelProviderService.SetLibraryPreCompileSetCompletionPostProcess += LanguageModelMgrConsolidated_SetLibraryPreCompileContextCompletionPostProcess;
			LMServiceProvider.PreCompileService.SignatureChanged += LanguageModelMgrConsolidated_SignatureChanged;
			LMServiceProvider.PreCompileService.SignatureDeleted += LanguageModelMgrConsolidated_SignatureDeleted;
			LMServiceProvider.PreCompileService.SignatureInserted += LanguageModelMgrConsolidated_SignatureInserted;
			LMServiceProvider.PreCompileService.TaskConfigChanged += LanguageModelMgrConsolidated_TaskConfigChanged;
		}

		private void LanguageModelMgrConsolidated_AddDownloadCode(object sender, AddImplicitCodeEventArgs e)
		{
			this.AddDownloadCode?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_TaskConfigChanged(object sender, CompileEventArgs e)
		{
			this.TaskConfigChanged?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_SignatureInserted(object sender, SignatureChangedEventArgs e)
		{
			this.SignatureInserted?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_SignatureDeleted(object sender, SignatureChangedEventArgs e)
		{
			this.SignatureDeleted?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_SignatureChanged(object sender, SignatureChangedEventArgs e)
		{
			this.SignatureChanged?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_SetLibraryPreCompileContextCompletionPostProcess(object sender, SetLibraryPreCompileContextCompletionPostProcessEventArgs args)
		{
			this.SetLibraryPreCompileContextCompletionPostProcess?.Invoke(sender, args);
		}

		private void LanguageModelMgrConsolidated_LibraryContextDeleted(object sender, LibraryContextDeletedEventArgs e)
		{
			this.LibraryContextDeleted?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_IsHiddenVariableHandler(object sender, IsHiddenVariableEventArgs args)
		{
			this.IsHiddenVariableHandler?.Invoke(sender, args);
		}

		private void LanguageModelMgrConsolidated_FilterMessageOutput(object sender, FilterMessageOutputEventArgs e)
		{
			this.FilterMessageOutput?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_CompiledPOUInserted(object sender, CompiledPOUChangedEventArgs e)
		{
			this.CompiledPOUInserted?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_CompiledPOUDeleted(object sender, CompiledPOUChangedEventArgs e)
		{
			this.CompiledPOUDeleted?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_CompiledPOUChanged(object sender, CompiledPOUChangedEventArgs e)
		{
			this.CompiledPOUChanged?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_CodeChanged(object sender, CompileEventArgs e)
		{
			this.CodeChanged?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_BeforeMessageOutput(object sender, MessageOutputEventArgs e)
		{
			this.BeforeMessageOutput?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_BeforeLocation(object sender, CompileEventArgs e)
		{
			this.BeforeLocation?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_BeforeGenerateRelinkCode(object sender, BeforeGenerateRelinkCodeEventArgs e)
		{
			this.BeforeGenerateRelinkCode?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_BeforeGenerateCompiledCode(object sender, CompileEventArgs e)
		{
			this.BeforeGenerateCompiledCode?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_BeforeClearAll(object sender, EventArgs e)
		{
			this.BeforeClearAll?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AfterSimulationModeChanged(object sender, SimulationModeArgs args)
		{
			this.AfterSimulationModeChanged?.Invoke(sender, args);
		}

		private void LanguageModelMgrConsolidated_AfterMessageOutput(object sender, MessageOutputEventArgs e)
		{
			this.AfterMessageOutput?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AfterLocation(object sender, CompileEventArgs e)
		{
			this.AfterLocation?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AfterLazyLibraryLoad(object sender, EventArgs e)
		{
			this.AfterLazyLibraryLoad?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AfterGenerateGlobalInitCode(object sender, AfterGenerateGlobalInitEventArgs e)
		{
			this.AfterGenerateGlobalInitCode?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AfterGenerateCode(object sender, CompileEventArgs e)
		{
			this.AfterGenerateCode?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AfterCompile(object sender, CompileEventArgs e)
		{
			this.AfterCompile?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AfterClearAll(object sender, EventArgs e)
		{
			this.AfterClearAll?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AddOnlineChangeCode(object sender, AddImplicitCodeEventArgs e)
		{
			this.AddOnlineChangeCode?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AddLateLanguageModel(object sender, AddLanguageModelEventArgs e)
		{
			this.AddLateLanguageModel?.Invoke(sender, e);
		}

		private void LanguageModelMgrConsolidated_AddGlobalInitCode(object sender, AddImplicitCodeEventArgs e)
		{
			this.AddGlobalInitCode?.Invoke(sender, e);
		}

		public IPreCompileContext[] AllPreCompileContexts(bool bWithDevices, bool bWithLibraries)
		{
			return LMServiceProvider.PreCompileService.AllPreCompileSets(bWithDevices, bWithLibraries).Cast<IPreCompileContext>().ToArray();
		}

		public ISignature[] AllPrecompiledSignatures(bool bWithLibraries, bool bWithResources)
		{
			return LMServiceProvider.PreCompileService.AllPrecompiledSignatures(bWithLibraries, bWithResources).ToArray();
		}

		public IScope AllSignatures(bool bCompiled)
		{
			return null;
		}

		public IApplicationContent BuildApplicationContentFromUpload(byte[] bytes)
		{
			return LMServiceProvider.CompileService.BuildApplicationContentFromUpload(bytes);
		}

		public IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder)
		{
			return LMServiceProvider.CompileService.BuildApplicationContentFromUpload(bytes, bIsMotorolaByteOrder);
		}

		public IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder, bool bByteSupport)
		{
			return LMServiceProvider.CompileService.BuildApplicationContentFromUpload(bytes, bIsMotorolaByteOrder, bByteSupport);
		}

		public int CalculatePointerSize(Guid guidApplication)
		{
			return LMServiceProvider.PreCompileStorageSizeEstimatorService.CalculatePointerSize(guidApplication);
		}

		public bool CanConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder)
		{
			return LMServiceProvider.MonitoringService.CanConvertRaw(raw, typeVarRef, guidApplication, byteOrder);
		}

		public bool CheckAllApplicationObjects(Guid guidApplication)
		{
			return LMServiceProvider.CommandService.CheckAllApplicationObjects(guidApplication);
		}

		public bool CheckAndLoadBootInfo(Guid guidApplication, Guid guidCode, Guid guidData)
		{
			return LMServiceProvider.CommandService.CheckAndLoadBootInfo(guidApplication, guidCode, guidData);
		}

		public IMessage[] CheckInterfaceLibraryCompatibility(IPreCompileContext pccOlderLibrary, IPreCompileContext pccNewerLibrary)
		{
			return LMServiceProvider.CommandService.CheckInterfaceLibraryCompatibility(pccOlderLibrary as ILMPreCompileSet, pccNewerLibrary as ILMPreCompileSet).ToArray();
		}

		public IMessage[] CheckLibraryCompatibility(IPreCompileContext pccOlderLibrary, IPreCompileContext pccNewerLibrary, bool bInterfaceLibrary)
		{
			return LMServiceProvider.CommandService.CheckLibraryCompatibility(pccOlderLibrary as ILMPreCompileSet, pccNewerLibrary as ILMPreCompileSet, bInterfaceLibrary).ToArray();
		}

		public void ClearAll()
		{
			LMServiceProvider.CommandService.ClearAll();
		}

		public void ClearAll(bool bAtProjectClose)
		{
			LMServiceProvider.CommandService.ClearAll(bAtProjectClose);
		}

		public void ClearDownloadContext(Guid guidApplication)
		{
			LMServiceProvider.CommandService.ClearDownloadInfo(guidApplication);
		}

		public bool Compile(Guid guidApplication)
		{
			return LMServiceProvider.CommandService.Compile(guidApplication);
		}

		public void CompileAll()
		{
			LMServiceProvider.CommandService.CompileAll();
		}

		public bool CompileAndLocate(Guid guidApplication)
		{
			return LMServiceProvider.CommandService.CompileAndLocate(guidApplication);
		}

		public object ConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder)
		{
			return LMServiceProvider.MonitoringService.ConvertRaw(raw, typeVarRef, guidApplication, byteOrder);
		}

		public byte[] ConvertToRaw(object value, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder)
		{
			return LMServiceProvider.MonitoringService.ConvertToRaw(value, typeVarRef, guidApplication, byteOrder);
		}

		public IAddressCalculation CreateAddressCalculaton(ICompileContext comcon)
		{
			return LMServiceProvider.CompileService.CreateAddressCalculaton(comcon.ApplicationGuid);
		}

		public void CreateBootDuplicate(Guid guidApplication)
		{
			LMServiceProvider.CommandService.CreateBootDuplicate(guidApplication);
		}

		public ILanguageModelBuilder CreateLanguageModelBuilder()
		{
			return LMServiceProvider.CreatorService.CreateLanguageModelBuilder();
		}

		public IParser CreateParser(IScanner scanner)
		{
			return LMServiceProvider.CreatorService.CreateParser(scanner);
		}

		public IScanner CreateScanner(string stText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces)
		{
			return LMServiceProvider.CreatorService.CreateScanner(stText, bIncludeComments, bIncludeEndOfLines, bIncludePragmas, bIncludeWhitespaces);
		}

		public IScope CreateScope(ISignature sign, Guid guidApplication)
		{
			return (LMServiceProvider.CompileService.GetTypificator(guidApplication) ?? LMServiceProvider.DownloadedApplicationService.GetTypificator(guidApplication))?.CreateScope(sign);
		}

		public IScope CreateScope(ISignature isign, ICollection Signatures, Guid guidApplication)
		{
			return (LMServiceProvider.CompileService.GetTypificator(guidApplication) ?? LMServiceProvider.DownloadedApplicationService.GetTypificator(guidApplication))?.CreateScope(isign);
		}

		public IExpressionTypifier CreateTypifier(Guid guidApplication, int idSignature)
		{
			return (LMServiceProvider.CompileService.GetTypificator(guidApplication) ?? LMServiceProvider.DownloadedApplicationService.GetTypificator(guidApplication))?.CreateTypifier(idSignature);
		}

		public IExpressionTypifier CreateTypifier(Guid guidApplication, int idSignature, bool bContributeToCompile, bool bInterpretPragmas)
		{
			ILMCompiledApplicationTypification iLMCompiledApplicationTypification = LMServiceProvider.CompileService.GetTypificator(guidApplication) ?? LMServiceProvider.DownloadedApplicationService.GetTypificator(guidApplication);
			if (iLMCompiledApplicationTypification != null)
			{
				return iLMCompiledApplicationTypification.CreateTypifier(idSignature, bContributeToCompile, bInterpretPragmas);
			}
			return ((ILMObsoleteService2)LMServiceProvider.ObsoleteService)?.CreateTypifier(guidApplication, idSignature, bContributeToCompile, bInterpretPragmas);
		}

		public IExpressionTypifier CreateTypifier(Guid guidApplication, IScope scope, bool bContributeToCompile, bool bInterpretPragmas)
		{
			return (LMServiceProvider.CompileService.GetTypificator(guidApplication) ?? LMServiceProvider.DownloadedApplicationService.GetTypificator(guidApplication))?.CreateTypifier(scope, bContributeToCompile, bInterpretPragmas);
		}

		public void DisablePrecompileChecksInNoUIMode()
		{
			LMServiceProvider.PreCompileCrossReferenceService.DisablePrecompileChecksInNoUIMode();
		}

		public void EnablePrecompileChecksInNoUIMode()
		{
			LMServiceProvider.PreCompileCrossReferenceService.EnablePrecompileChecksInNoUIMode();
		}

		public void EnqueueSetLibraryPreCompileContextFromArchive(Stream stream, Guid readerGuid, string stLibraryId, ISharedDataStorage sharedDataStorage, SetLibraryPreCompileContextCompletionEventHandler completionEventHandler, object callerData)
		{
			LMServiceProvider.LanguageModelProviderService.EnqueueSetLibraryPreCompileSetFromArchive(stream, readerGuid, stLibraryId, sharedDataStorage, completionEventHandler, callerData);
		}

		public bool ExecutionpointLoggingEnabled(Guid appGuid)
		{
			return LMServiceProvider.ConfigurationService.ExecutionpointLoggingEnabled(appGuid);
		}

		public bool ExpressionsEqual(IExprement exp1, IExprement exp2)
		{
			return LMServiceProvider.PreCompileService.ExpressionsEqual(exp1, exp2);
		}

		public IExprement FindExpressionAtSourcePosition(ISourcePosition sourcepos, WhatToFind whattofind, out IPreCompileContext precom)
		{
			ILMPreCompileSet precom2;
			IExprement result = LMServiceProvider.PreCompileSmartCodingService.FindExpressionAtSourcePosition(sourcepos, whattofind, out precom2);
			precom = precom2 as IPreCompileContext;
			return result;
		}

		public ISignature FindSignature(Guid guidObject, out IPreCompileContext precom)
		{
			ILMPreCompileSet precom2;
			ISignature result = LMServiceProvider.PreCompileService.FindSignature(guidObject, out precom2);
			precom = precom2 as IPreCompileContext;
			return result;
		}

		public ISignature[] FindSignaturesByName(int nProjectHandle, Guid callingObjectGuid, string stName)
		{
			return LMServiceProvider.PreCompileService.FindSignaturesByName(nProjectHandle, callingObjectGuid, stName).ToArray();
		}

		public ISignature[] FindSignaturesByName(int nProjectHandle, Guid applicationGuid, Guid callingObjectGuid, string stName)
		{
			return LMServiceProvider.PreCompileService.FindSignaturesByName(nProjectHandle, applicationGuid, callingObjectGuid, stName).ToArray();
		}

		public void FinishPrecompileChecks()
		{
			LMServiceProvider.PreCompileCrossReferenceService.FinishPrecompileChecks();
		}

		public void ForceRebuildAll(Guid guidApplication)
		{
			LMServiceProvider.CommandService.ForceRebuildAll(guidApplication);
		}

		public bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation)
		{
			return LMServiceProvider.CommandService.GenerateCode(guidApplication, bOnlineChange, bKeepCompileInformation);
		}

		public bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation, out IMessage[] errors, out IMessage[] warnings)
		{
			return LMServiceProvider.CommandService.GenerateCode(guidApplication, bOnlineChange, bKeepCompileInformation, out errors, out warnings);
		}

		public bool GenerateCodeForSystemApp(Guid guidApplication, string stLibraryId)
		{
			return false;
		}

		public bool GenerateOnlineChangeCode(Guid guidApplication, bool bKeepCompileInformation, out IOnlineChangeDetails ocd)
		{
			return LMServiceProvider.CommandService.GenerateOnlineChangeCode(guidApplication, bKeepCompileInformation, out ocd);
		}

		public IAddressInfo GetAddressInfo(Guid guidApplication, IExpression exp, IScope scope)
		{
			return LMServiceProvider.MonitoringService.GetAddressInfo(guidApplication, exp, scope);
		}

		public IList<IDirectVariableAccess> GetAllDirectVariableAccesses()
		{
			return LMServiceProvider.PreCompileCrossReferenceService.GetAllDirectVariableAccesses().ToList();
		}

		public IFlowVarRef[] GetAllFlowVarReferences(string stInstance, long[] alPositionsOfInterest)
		{
			return LMServiceProvider.FlowMonitoringService.GetAllFlowVarReferences(stInstance, alPositionsOfInterest).ToArray();
		}

		public IFlowVarRef[] GetAllFlowVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest)
		{
			return LMServiceProvider.FlowMonitoringService.GetAllFlowVarReferences(objectguid, guidExplicitApplicationGuid, stInstance, alPositionsOfInterest).ToArray();
		}

		public ISignature2[] GetAllInterfaces(ISignature2 sign)
		{
			return LMServiceProvider.PreCompileService.GetAllInterfaces(sign).ToArray();
		}

		public ISignature2[] GetAllInterfaces(ISignature2 sign, Guid guidApplication)
		{
			return LMServiceProvider.PreCompileService.GetAllInterfaces(sign, guidApplication).ToArray();
		}

		public ISignature2[] GetAllMethods(ISignature2 sign)
		{
			return LMServiceProvider.PreCompileService.GetAllMethods(sign).ToArray();
		}

		public ISignature2[] GetAllMethods(ISignature2 sign, Guid guidApplication)
		{
			return LMServiceProvider.PreCompileService.GetAllMethods(sign, guidApplication).ToArray();
		}

		public IVariable2[] GetAllVariables(ISignature2 sign)
		{
			return LMServiceProvider.PreCompileService.GetAllVariables(sign).ToArray();
		}

		public IVariable2[] GetAllVariables(ISignature2 sign, Guid guidApplication)
		{
			return LMServiceProvider.PreCompileService.GetAllVariables(sign, guidApplication).ToArray();
		}

		public IVarRef[] GetAllVarReferences(string stInstance, long[] alPositionsOfInterest)
		{
			return LMServiceProvider.MonitoringService.GetAllVarReferences(stInstance, alPositionsOfInterest).ToArray();
		}

		public IVarRef[] GetAllVarReferences(Guid objectguid, string stInstance, long[] alPositionsOfInterest)
		{
			return LMServiceProvider.MonitoringService.GetAllVarReferences(objectguid, stInstance, alPositionsOfInterest).ToArray();
		}

		public IVarRef[] GetAllVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest)
		{
			return LMServiceProvider.MonitoringService.GetAllVarReferences(objectguid, guidExplicitApplicationGuid, stInstance, alPositionsOfInterest).ToArray();
		}

		public Guid GetApplicationGuidByName(string stName)
		{
			return LMServiceProvider.LanguageModelProviderService.GetApplicationGuidByName(stName);
		}

		public string GetApplicationNameByGuid(Guid guidApplication)
		{
			return LMServiceProvider.LanguageModelProviderService.GetApplicationNameByGuid(guidApplication);
		}

		public string GetApplicationNameByGuid(Guid guidApplication, bool bSimulationMode)
		{
			EQueryApplicationNameFlags eFlags = (bSimulationMode ? EQueryApplicationNameFlags.SimulationMode : EQueryApplicationNameFlags.Default);
			return LMServiceProvider.LanguageModelProviderService.GetApplicationNameByGuid(guidApplication, eFlags);
		}

		public ulong[] GetAreaSizesAfterShrinking(Guid appGuid)
		{
			return LMServiceProvider.CompileService.GetAreaSizesAfterShrinking(appGuid);
		}

		public ISignature2 GetBaseSignature(ISignature2 sign)
		{
			return LMServiceProvider.PreCompileService.GetBaseSignature(sign);
		}

		public ISignature2 GetBaseSignature(ISignature2 sign, Guid guidApplication)
		{
			return LMServiceProvider.PreCompileService.GetBaseSignature(sign, guidApplication);
		}

		public IBreakPointTable GetBreakPointTable(string stPOUName)
		{
			return null;
		}

		public ICompileContext GetCompileContext(Guid guidApplication)
		{
			return LMServiceProvider.CompileService.GetCompiledApplicationSet(guidApplication) as ICompileContext;
		}

		public Guid GetCompileContextGuidByName(string stResourceName, string stApplicationName)
		{
			return LMServiceProvider.CompileService.GetCompiledApplicationSetGuidByName(stResourceName, stApplicationName);
		}

		public void GetCompiledIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId)
		{
			LMServiceProvider.CompileService.GetCompiledIds(guidApplication, out guidCodeId, out guidDataId);
		}

		public IMessage[] GetCompilerMessages(Guid guidApplication)
		{
			return Array.Empty<IMessage>();
		}

		public IConverterFromIEC GetConverterFromIEC()
		{
			return LMServiceProvider.MonitoringService.GetConverterFromIEC();
		}

		public IConverterToIEC GetConverterToIEC(bool bOmitPrefixesWherePossible, bool bUseShortPrefixes, DisplayMode displayMode)
		{
			return LMServiceProvider.MonitoringService.GetConverterToIEC(bOmitPrefixesWherePossible, bUseShortPrefixes, displayMode);
		}

		public string GetDefaultInitializationCode(IVariable var, ISignature sign, IScope scope, string stInstancePath)
		{
			Guid applicationGuid = ((IScope5)scope).ApplicationContext.ApplicationGuid;
			return (LMServiceProvider.DownloadedApplicationService.QueryCompiledApplicationSet(applicationGuid) ?? LMServiceProvider.CompileService.QueryCompiledApplicationSet(applicationGuid)).GetDefaultInitializationCode(var, sign, scope, stInstancePath);
		}

		public IAccessInfo[] GetDirectVariableAccess(IDirectVariable dirvar, bool bCompiled)
		{
			return LMServiceProvider.PreCompileCrossReferenceService.GetDirectVariableAccess(dirvar).ToArray();
		}

		public void GetDownloadIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId)
		{
			LMServiceProvider.DownloadedApplicationService.GetDownloadIds(guidApplication, out guidCodeId, out guidDataId);
		}

		public void GetLastDownloadIds(Guid guidApplication, out Guid guidCodeIdLast, out Guid guidDataIdLast)
		{
			LMServiceProvider.DownloadedApplicationService.GetInitialDownloadIds(guidApplication, out guidCodeIdLast, out guidDataIdLast);
		}

		public IDownloadInfo GetDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject)
		{
			return LMServiceProvider.CompileService.GetDownloadInfo(guidApplication, bOnlineChange, bBootProject);
		}

		public IExternalReference[] GetExternalReferences(ICompileContext comcon, bool bCompactDownload)
		{
			return LMServiceProvider.CompileService.GetExternalReferences(comcon.ApplicationGuid, bCompactDownload).ToArray();
		}

		public IFlowVarRef GetFlowVarReference(string stExpression, string stInstancePath, long lPosition)
		{
			return LMServiceProvider.FlowMonitoringService.GetFlowVarReference(stExpression, stInstancePath, lPosition);
		}

		public int GetGranularity(IPreCompileContext precom, ICompiledType type)
		{
			return LMServiceProvider.PreCompileStorageSizeEstimatorService.GetGranularity(precom as ILMPreCompileSet, type);
		}

		public IIdentifierInfo[] GetIdentifierInfoAtSourcePosition(string stName, ISourcePosition sourcepos, WhatToFind whattofind)
		{
			IEnumerable<IIdentifierInfo> identifierInfoAtSourcePosition = LMServiceProvider.PreCompileSmartCodingService.GetIdentifierInfoAtSourcePosition(stName, sourcepos, whattofind);
			IIdentifierInfo[] result = null;
			if (identifierInfoAtSourcePosition != null)
			{
				result = identifierInfoAtSourcePosition.ToArray();
			}
			return result;
		}

		public byte[] GetInitializationBlob(IScope5 scope, bool isMotorolaByteOrder, IVariable var, out IRelocationList2 relocations)
		{
			Guid applicationGuid = scope.ApplicationContext.ApplicationGuid;
			return (LMServiceProvider.DownloadedApplicationService.QueryCompiledApplicationSet(applicationGuid) ?? LMServiceProvider.CompileService.QueryCompiledApplicationSet(applicationGuid)).GetInitializationBlob(scope, isMotorolaByteOrder, var, out relocations);
		}

		public ISignature2[] GetInterfaceSignatures(ISignature2 sign)
		{
			return LMServiceProvider.PreCompileService.GetInterfaceSignatures(sign).ToArray();
		}

		public ISignature2[] GetInterfaceSignatures(ISignature2 sign, Guid guidApplication)
		{
			return LMServiceProvider.PreCompileService.GetInterfaceSignatures(sign, guidApplication).ToArray();
		}

		public IPreCompileContext GetLibraryPrecompileContext(string stLibraryId)
		{
			return LMServiceProvider.PreCompileService.GetLibraryPrecompileSet(stLibraryId) as IPreCompileContext;
		}

		public IDownloadInfo GetOfflineBootProjectInfo(Guid guidApplication)
		{
			return LMServiceProvider.CompileService.GetOfflineBootProjectInfo(guidApplication);
		}

		public IDownloadInfo GetOnlineBootProjectInfo(Guid guidApplication)
		{
			return LMServiceProvider.CompileService.GetOnlineBootProjectInfo(guidApplication);
		}

		public Guid GetParentApplicationGuid(Guid guidApplication)
		{
			return LMServiceProvider.LanguageModelProviderService.GetParentApplicationGuid(guidApplication);
		}

		public IAccessInfo[] GetPOUAccess(string stPOUName, bool bCompiled)
		{
			return LMServiceProvider.PreCompileCrossReferenceService.GetPOUAccess(stPOUName).ToArray();
		}

		public IPreCompileContext GetPrecompileContext(Guid guidApplication)
		{
			return LMServiceProvider.PreCompileService.GetPreCompileSet(guidApplication) as IPreCompileContext;
		}

		public IPreCompileContext2 GetPrecompileContextOfSignature(ISignature sign)
		{
			return (IPreCompileContext2)LMServiceProvider.PreCompileService.GetPrecompileSetOfSignature(sign);
		}

		public IDictionary<string, IList<IAccessInfo>> GetPrecompiledCrossReferences(Regex regex)
		{
			return LMServiceProvider.PreCompileCrossReferenceService.GetPrecompiledCrossReferences(regex);
		}

		public ICompileContext GetReferenceContextIfAvailable(Guid guidApplication)
		{
			return LMServiceProvider.DownloadedApplicationService.GetCompiledApplicationSet(guidApplication) as ICompileContext;
		}

		public ISignature6 GetSignatureForPrecompileID(int precompileId)
		{
			return LMServiceProvider.PreCompileService.GetSignatureForPrecompileID(precompileId);
		}

		public Guid[] GetSubApplicationGuids(Guid guidApplication, bool bRecursive)
		{
			return LMServiceProvider.LanguageModelProviderService.GetSubApplicationGuids(guidApplication, bRecursive).ToArray();
		}

		public IList<ITaskCrossref> GetTaskReferencesOfInstancePath(string stInstancePath)
		{
			return LMServiceProvider.CompileService.GetTaskReferencesOfInstancePath(stInstancePath).ToList();
		}

		public IType GetTypeOfLiteral(ILiteralExpression litExp, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32)
		{
			return LMServiceProvider.CompileService.GetTypeOfLiteral(litExp, bLRealSupported, bTreatLRealAsReal, bInt64Supported, bTreatInt64AsInt32);
		}

		public IType GetTypeOfLiteral(ILiteralExpression litExp, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32, bool bUnicodeNotSupported)
		{
			return LMServiceProvider.CompileService.GetTypeOfLiteral(litExp, bLRealSupported, bTreatLRealAsReal, bInt64Supported, bTreatInt64AsInt32, bUnicodeNotSupported);
		}

		public IVariable GetVariableCompiled(string stVariableName)
		{
			return LMServiceProvider.CompileService.GetVariable(stVariableName);
		}

		public IAccessInfo[] GetVariableAccess(string stVariableName, bool bCompiled)
		{
			return LMServiceProvider.PreCompileCrossReferenceService.GetVariableAccess(stVariableName).ToArray();
		}

		public IVariable GetVariable(string stVariableName, bool bCompiled)
		{
			return LMServiceProvider.DownloadedApplicationService.GetVariable(stVariableName);
		}

		public IVarRef GetVarReference(string stExpression)
		{
			return LMServiceProvider.MonitoringService.GetVarReference(stExpression);
		}

		public IVarRef GetVarReference(string stExpression, string stInstancePath)
		{
			return LMServiceProvider.MonitoringService.GetVarReference(stExpression, stInstancePath);
		}

		public IVarRef GetVarReference(Guid guidApplication, string stExpression)
		{
			return LMServiceProvider.MonitoringService.GetVarReference(guidApplication, stExpression);
		}

		public IVarRef GetVarReference(Guid guidApplication, string stExpression, bool bAllowShortExpressions)
		{
			return LMServiceProvider.MonitoringService.GetVarReference(guidApplication, stExpression, bAllowShortExpressions);
		}

		public IVarRef GetVarReference(Guid guidApplication, string stPOUName, string stExpression)
		{
			return LMServiceProvider.MonitoringService.GetVarReference(guidApplication, stPOUName, stExpression);
		}

		public IVarRef GetVarReference(Guid guidApplication, string stInstancePath, string stExpression, int nProjectHandle, Guid guidObject)
		{
			return LMServiceProvider.MonitoringService.GetVarReference(guidApplication, stInstancePath, stExpression, nProjectHandle, guidObject);
		}

		public IScope GlobalSignatures(bool bCompiled)
		{
			return null;
		}

		public bool IsExcludedFromBuild(int projectHandle, Guid objectGuid)
		{
			return LMServiceProvider.LanguageModelProviderService.IsExcludedFromBuild(projectHandle, objectGuid);
		}

		public bool IsExcludedFromBuild(int projectHandle, Guid objectGuid, out bool inherited)
		{
			return LMServiceProvider.LanguageModelProviderService.IsExcludedFromBuild(projectHandle, objectGuid, out inherited);
		}

		public bool IsHiddenSignature(ISignature signature, GUIHidingFlags flagsToConsider)
		{
			return LMServiceProvider.PreCompileService.IsHiddenSignature(signature, flagsToConsider);
		}

		public bool IsHiddenVariable(IVariable variable, GUIHidingFlags flagsToConsider)
		{
			return LMServiceProvider.ObsoleteService.IsHiddenVariable(variable, flagsToConsider);
		}

		public bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider)
		{
			return LMServiceProvider.PreCompileService.IsHiddenVariable(signature, variable, flagsToConsider);
		}

		public bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider, ISignature signCurrent)
		{
			return LMServiceProvider.PreCompileService.IsHiddenVariable(signature, variable, flagsToConsider, signCurrent);
		}

		public bool IsResolvedXType(IType type)
		{
			return ((ITypeInfo4)LMServiceProvider.CreatorService.TypeInfo).IsResolvedXType(type);
		}

		public bool IsUpToDate(Guid guidApplication)
		{
			return LMServiceProvider.DownloadedApplicationService.IsUpToDate(guidApplication);
		}

		public bool IsUpToDate(Guid guidApplication, out bool bOnlineChangePossible)
		{
			return LMServiceProvider.DownloadedApplicationService.IsUpToDate(guidApplication, out bOnlineChangePossible);
		}

		public void OnSaveProjectAs(string stOldProjectPath, string stNewProjectPath)
		{
			LMServiceProvider.CommandService.OnSaveProjectAs(stOldProjectPath, stNewProjectPath);
		}

		public void ProcessQueuedLibraryPreCompileContexts(IProgressCallback callback)
		{
			LMServiceProvider.LanguageModelProviderService.ProcessQueuedLibraryPreCompileSets(callback);
		}

		public void ProcessQueuedLibraryPreCompileContexts(bool bWaitForCompletion)
		{
			LMServiceProvider.LanguageModelProviderService.ProcessQueuedLibraryPreCompileSets(bWaitForCompletion);
		}

		public void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors)
		{
			LMServiceProvider.LanguageModelProviderService.PutLanguageModel(lanmodprov, bShowSyntaxErrors);
		}

		public void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors, bool forceCompleteLanguageModel)
		{
			LMServiceProvider.LanguageModelProviderService.PutLanguageModel(lanmodprov, bShowSyntaxErrors, forceCompleteLanguageModel);
		}

		public void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid)
		{
			LMServiceProvider.LanguageModelProviderService.RemoveLanguageModelOfObject(nProjectHandle, objectGuid);
		}

		public void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid, bool bShowPrecompileErrors)
		{
			LMServiceProvider.LanguageModelProviderService.RemoveLanguageModelOfObject(nProjectHandle, objectGuid, bShowPrecompileErrors);
		}

		public void RemoveLanguageModelOfProject(string stProjectId)
		{
			LMServiceProvider.LanguageModelProviderService.RemoveLanguageModelOfProject(stProjectId);
		}

		public void SavePreCompileContextToArchive(IPreCompileContext2 precom, IArchiveWriter writer)
		{
			LMServiceProvider.CommandService.SavePreCompileSetToArchive(precom as ILMPreCompileSet, writer);
		}

		public void SavePreCompileContextToArchive(IPreCompileContext2 precom, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage, Profile profile, IArchiveReporter reporter)
		{
			LMServiceProvider.CommandService.SavePreCompileSetToArchive(precom as ILMPreCompileSet, writer, sharedDataStorage, profile, reporter);
		}

		public void SaveToProject(int nProjectHandle)
		{
			LMServiceProvider.CommandService.SaveToProject(nProjectHandle);
		}

		public IPreCompileContext2 SetLibraryPreCompileContextFromArchive(IArchiveReader reader, string stLibraryId)
		{
			return LMServiceProvider.LanguageModelProviderService.SetLibraryPreCompileSetFromArchive(reader, stLibraryId) as IPreCompileContext2;
		}

		public IPreCompileContext2 SetLibraryPreCompileContextFromArchive(IArchiveReader2 reader, string stLibraryId, ISharedDataStorage sharedDataStorage)
		{
			return LMServiceProvider.LanguageModelProviderService.SetLibraryPreCompileSetFromArchive(reader, stLibraryId, sharedDataStorage) as IPreCompileContext2;
		}

		public void SimulationModeChanged(Guid guidDevice)
		{
			LMServiceProvider.CommandService.SimulationModeChanged(guidDevice);
		}

		public void UpdateDownloadContext(Guid guidApplication)
		{
			LMServiceProvider.CommandService.UpdateDownloadInfoAsync(guidApplication);
		}

		public void UpdateDownloadContext(Guid guidApplication, bool bCreateBootDuplicate)
		{
			LMServiceProvider.CommandService.UpdateDownloadInfoAsync(guidApplication, bCreateBootDuplicate);
		}

		public void UpdateDownloadContextSynchWriteContext(Guid guidApplication)
		{
			LMServiceProvider.CommandService.UpdateDownloadInfoSync(guidApplication);
		}

		public void UpdateDownloadContextSynchWriteContext(Guid guidApplication, bool bCreateBootDuplicate)
		{
			LMServiceProvider.CommandService.UpdateDownloadInfoSync(guidApplication, bCreateBootDuplicate);
		}

		public void UpdateDownloadContextWithoutWriteContext(Guid guidApplication)
		{
			LMServiceProvider.CommandService.UpdateDownloadInfoInMemoryOnly(guidApplication);
		}
	}
}
