using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager21
	{
		IPreCompileContext[] PrecompileContexts { get; }

		IPreCompileContext[] LibraryContexts { get; }

		ICompileContext[] CompileContexts { get; }

		ICompileContext[] ReferenceContexts { get; }

		ITypeInfo TypeInfo { get; }

		int PendingLibrariesInLoadQueue { get; }

		bool DelayedLoaderWorking { get; }

		event BeforeMessageOutputEventHandler BeforeMessageOutput;

		event FilterMessageOutputEventHandler FilterMessageOutput;

		event AfterMessageOutputEventHandler AfterMessageOutput;

		event CompileEventHandler BeforeCompile;

		event CompileEventHandler AfterCompile;

		event CompileEventHandler CodeChanged;

		event EventHandler BeforeClearAll;

		event EventHandler AfterClearAll;

		event AddLanguageModelEventHandler AddLateLanguageModel;

		event CompileEventHandler BeforeLocation;

		event CompileEventHandler AfterLocation;

		event CompileEventHandler TaskConfigChanged;

		event SignatureChangedEventHandler SignatureChanged;

		event SignatureChangedEventHandler SignatureDeleted;

		event SignatureChangedEventHandler SignatureInserted;

		event CompiledPOUChangedEventHandler CompiledPOUChanged;

		event CompiledPOUChangedEventHandler CompiledPOUDeleted;

		event CompiledPOUChangedEventHandler CompiledPOUInserted;

		event AddImplicitCodeEventHandler AddDownloadCode;

		event AddImplicitCodeEventHandler AddGlobalInitCode;

		event AddImplicitCodeEventHandler AddOnlineChangeCode;

		event EventHandler AfterLazyLibraryLoad;

		event SetLibraryPreCompileContextCompletionPostProcessEventHandler SetLibraryPreCompileContextCompletionPostProcess;

		event CompileEventHandler AfterGenerateCode;

		event IsHiddenVariableEventHandler IsHiddenVariableHandler;

		event SimulationModeChangedEventHandler AfterSimulationModeChanged;

		void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors);

		void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid);

		void RemoveLanguageModelOfProject(string stProjectId);

		bool IsUpToDate(Guid guidApplication, out bool bOnlineChangePossible);

		ISignature[] AllPrecompiledSignatures(bool bWithLibraries, bool bWithResources);

		IPreCompileContext[] AllPreCompileContexts(bool bWithDevices, bool bWithLibraries);

		[Obsolete("Returns Null! Use ICompileContextCommon.AllSignatures")]
		IScope AllSignatures(bool bCompiled);

		[Obsolete("Returns Null! Use ICompileContextCommon.GVLSignatures")]
		IScope GlobalSignatures(bool bCompiled);

		IScope CreateScope(ISignature isign, ICollection Signatures, Guid guidApplication);

		IScope CreateScope(ISignature sign, Guid guidApplication);

		IAccessInfo[] GetVariableAccess(string stVariableName, bool bCompiled);

		IAccessInfo[] GetPOUAccess(string stPOUName, bool bCompiled);

		IAccessInfo[] GetDirectVariableAccess(IDirectVariable dirvar, bool bCompiled);

		IVariable GetVariable(string stVariableName, bool bCompiled);

		ISignature FindSignature(Guid guidObject, out IPreCompileContext precom);

		ISignature[] FindSignaturesByName(int nProjectHandle, Guid callingObjectGuid, string stName);

		[Obsolete("Use ICompileContext.BreakpointList")]
		IBreakPointTable GetBreakPointTable(string stPOUName);

		[Obsolete("Returns Null! Use ICompiledPOU7.GetMessages")]
		IMessage[] GetCompilerMessages(Guid guidApplication);

		void ClearAll();

		void SaveToProject(int nProjectHandle);

		void ForceRebuildAll(Guid guidApplication);

		void ClearDownloadContext(Guid guidApplication);

		void CompileAll();

		bool Compile(Guid guidApplication);

		void UpdateDownloadContext(Guid guidApplication);

		void GetCompiledIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId);

		void GetDownloadIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId);

		void GetLastDownloadIds(Guid guidApplication, out Guid guidCodeIdLast, out Guid guidDataIdLast);

		IDownloadInfo GetDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject);

		bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation);

		IVarRef GetVarReference(Guid guidApplication, string stPOUName, string stExpression);

		IVarRef GetVarReference(Guid guidApplication, string stExpression);

		IVarRef GetVarReference(string stExpression);

		IVarRef[] GetAllVarReferences(string stInstance, long[] alPositionsOfInterest);

		IVarRef[] GetAllVarReferences(Guid objectguid, string stInstance, long[] alPositionsOfInterest);

		IConverterFromIEC GetConverterFromIEC();

		IConverterToIEC GetConverterToIEC(bool bOmitPrefixesWherePossible, bool bUseShortPrefixes, DisplayMode displayMode);

		bool CanConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder);

		object ConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder);

		byte[] ConvertToRaw(object value, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder);

		IScanner CreateScanner(string stText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces);

		IParser CreateParser(IScanner scanner);

		IExpressionTypifier CreateTypifier(Guid guidApplication, int idSignature);

		IPreCompileContext GetPrecompileContext(Guid guidApplication);

		ICompileContext GetCompileContext(Guid guidApplication);

		Guid GetCompileContextGuidByName(string stResourceName, string stApplicationName);

		IExprement FindExpressionAtSourcePosition(ISourcePosition sourcepos, WhatToFind whattofind, out IPreCompileContext precom);

		IIdentifierInfo[] GetIdentifierInfoAtSourcePosition(string stName, ISourcePosition sourcepos, WhatToFind whattofind);

		string GetApplicationNameByGuid(Guid guidApplication);

		Guid GetApplicationGuidByName(string stName);

		ICompileContext GetReferenceContextIfAvailable(Guid guidApplication);

		IPreCompileContext2 GetPrecompileContextOfSignature(ISignature sign);

		ISignature2 GetBaseSignature(ISignature2 sign);

		ISignature2[] GetInterfaceSignatures(ISignature2 sign);

		IVariable2[] GetAllVariables(ISignature2 sign);

		ISignature2[] GetAllMethods(ISignature2 sign);

		ISignature2[] GetAllInterfaces(ISignature2 sign);

		IDownloadInfo GetOfflineBootProjectInfo(Guid guidApplication);

		IDownloadInfo GetOnlineBootProjectInfo(Guid guidApplication);

		void SavePreCompileContextToArchive(IPreCompileContext2 precom, IArchiveWriter writer);

		IPreCompileContext2 SetLibraryPreCompileContextFromArchive(IArchiveReader reader, string stLibraryId);

		void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid, bool bShowPrecompileErrors);

		void ClearAll(bool bAtProjectClose);

		Guid[] GetSubApplicationGuids(Guid guidApplication, bool bRecursive);

		Guid GetParentApplicationGuid(Guid guidApplication);

		IVariable GetVariableCompiled(string stVariableName);

		string GetDefaultInitializationCode(IVariable var, ISignature sign, IScope scope, string stInstancePath);

		void UpdateDownloadContextSynchWriteContext(Guid guidApplication);

		ISignature2 GetBaseSignature(ISignature2 sign, Guid guidApplication);

		ISignature2[] GetInterfaceSignatures(ISignature2 sign, Guid guidApplication);

		IVariable2[] GetAllVariables(ISignature2 sign, Guid guidApplication);

		ISignature2[] GetAllMethods(ISignature2 sign, Guid guidApplication);

		ISignature2[] GetAllInterfaces(ISignature2 sign, Guid guidApplication);

		bool GenerateCodeForSystemApp(Guid guidApplication, string stLibraryId);

		void SavePreCompileContextToArchive(IPreCompileContext2 precom, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage, Profile profile, IArchiveReporter reporter);

		IPreCompileContext2 SetLibraryPreCompileContextFromArchive(IArchiveReader2 reader, string stLibraryId, ISharedDataStorage sharedDataStorage);

		IExpressionTypifier CreateTypifier(Guid guidApplication, int idSignature, bool bContributeToCompile, bool bInterpretPragmas);

		void EnqueueSetLibraryPreCompileContextFromArchive(Stream stream, Guid readerGuid, string stLibraryId, ISharedDataStorage sharedDataStorage, SetLibraryPreCompileContextCompletionEventHandler completionEventHandler, object callerData);

		void ProcessQueuedLibraryPreCompileContexts(bool bWaitForCompletion);

		void ProcessQueuedLibraryPreCompileContexts(IProgressCallback callback);

		ISignature[] FindSignaturesByName(int nProjectHandle, Guid applicationGuid, Guid callingObjectGuid, string stName);

		IVarRef GetVarReference(Guid guidApplication, string stExpression, bool bAllowShortExpressions);

		void SimulationModeChanged(Guid guidDevice);

		bool GenerateOnlineChangeCode(Guid guidApplication, bool bKeepCompileInformation, out IOnlineChangeDetails ocd);

		void CreateBootDuplicate(Guid guidApplication);

		void UpdateDownloadContext(Guid guidApplication, bool bCreateBootDuplicate);

		bool CheckAndLoadBootInfo(Guid guidApplication, Guid guidCode, Guid guidData);

		IFlowVarRef[] GetAllFlowVarReferences(string stInstance, long[] alPositionsOfInterest);

		IFlowVarRef GetFlowVarReference(string stExpression, string stInstancePath, long lPosition);

		bool ExpressionsEqual(IExprement exp1, IExprement exp2);

		IType GetTypeOfLiteral(ILiteralExpression litExp, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32);

		bool CompileAndLocate(Guid guidApplication);

		bool CheckAllApplicationObjects(Guid guidApplication);

		IApplicationContent BuildApplicationContentFromUpload(byte[] bytes);

		bool IsHiddenVariable(IVariable variable, GUIHidingFlags flagsToConsider);

		bool IsHiddenSignature(ISignature signature, GUIHidingFlags flagsToConsider);

		bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation, out IMessage[] errors, out IMessage[] warnings);

		ILanguageModelBuilder CreateLanguageModelBuilder();

		IList<ITaskCrossref> GetTaskReferencesOfInstancePath(string stInstancePath);

		void OnSaveProjectAs(string stOldProjectPath, string stNewProjectPath);

		bool IsExcludedFromBuild(int projectHandle, Guid objectGuid);

		bool IsExcludedFromBuild(int projectHandle, Guid objectGuid, out bool inherited);

		IMessage[] CheckInterfaceLibraryCompatibility(IPreCompileContext pccOlderLibrary, IPreCompileContext pccNewerLibrary);

		IAddressCalculation CreateAddressCalculaton(ICompileContext comcon);

		IDictionary<string, IList<IAccessInfo>> GetPrecompiledCrossReferences(Regex regex);

		IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder);

		string GetApplicationNameByGuid(Guid guidApplication, bool bSimulationMode);

		void UpdateDownloadContextSynchWriteContext(Guid guidApplication, bool bCreateBootDuplicate);

		IVarRef GetVarReference(string stExpression, string stInstancePath);

		bool IsResolvedXType(IType type);

		IMessage[] CheckLibraryCompatibility(IPreCompileContext pccOlderLibrary, IPreCompileContext pccNewerLibrary, bool bInterfaceLibrary);

		IAddressInfo GetAddressInfo(Guid guidApplication, IExpression exp, IScope scope);

		IType GetTypeOfLiteral(ILiteralExpression litExp, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32, bool bUnicodeNotSupported);

		bool ExecutionpointLoggingEnabled(Guid appGuid);
	}
}
