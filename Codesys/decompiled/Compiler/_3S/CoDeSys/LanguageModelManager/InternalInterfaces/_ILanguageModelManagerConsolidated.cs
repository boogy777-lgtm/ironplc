using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILanguageModelManagerConsolidated : _ILanguageModelManagerLegacy
	{
		_IApplicationDeviceTable ApplicationDeviceTable { get; }

		_ILibraryList LibList { get; }

		IMessageCategory MessageCategory { get; }

		IMessageCategory PrecompileMessageCategory { get; }

		IPrecompileErrors PrecompileErrors { get; }

		IAttributeManager AttributeManager { get; }

		IMemorySettingsHelper MemorySettingsHelper { get; }

		IProgress Progress { get; }

		_IDelayedLoader DelayedLoader { get; }

		_IPreCompileContext Pool { get; }

		_IPreCompileContext[] Libraries { get; }

		IEnumerable<_IPreCompileContext> _PrecompileContexts { get; }

		_IPreCompileContext _SystemContext { get; }

		_ICompileContext this[Guid guidApplication] { get; set; }

		bool AllowNestedComments { get; }

		bool EnableBackgroundLoading { get; }

		bool EnablePrecomCheck { get; }

		bool ShowAllInstanceVars { get; }

		bool CompilationInProgress { get; }

		bool LateLibraryLoadFinished { get; }

		bool SuppressChangedEvents { get; set; }

		_IPreCompCrossReferences PCCRVariables { get; }

		_IPreCompCrossReferences PCCRCalls { get; }

		_IPreCompCrossReferences PCCRDirVars { get; }

		ICRCSum CreateCheckSumComputer();

		string VersionFreeLibraryPath(string stDisplayName);

		IScanner CreateScanner(IList<string> strlText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces);

		bool DoOutput(_ICompileContext comcon);

		IEnumerable<KeyValuePair<ISignature, IPreCompileContext>> FindSignatures(Guid guidObject);

		_IPreCompileContext _GetPrecompileContext(Guid guidApplication);

		void RemoveCompileContext(Guid guidApplication);

		_ICompileContext GetReferenceContextSynchronLoad(Guid guidApplication);

		void LoadReferenceContextInBackground(Guid guidApplication);

		_IPreCompileContext GetLibraryContext(string stLibraryId);

		IScanner5 CreateScanner();

		string GetApplicationFileNameNew(Guid guidApplication, bool bPrecompile, bool bSimulation);

		bool HasGlobalInitCode();

		bool HasOnlineChangeCode();

		bool HasDownloadCode();

		void StartCompilation();

		string GetDownloadCode(ICompileContext3 comcon3, ISignature sign);

		string GetOnlineChangeCode(ICompileContext3 comcon3, ISignature sign);

		ICompiledPOU FindPrecompiledPOU(Guid guidObject);

		void AddRelatedObject(Guid guidLanguageModelGlobalObject, Guid guidObject, Guid guidRelated);

		IEnumerable<Guid> GetRelatedObjects(Guid guidObject);

		Version GetRuntimeVersion(Guid guidApplication);

		void SetReferenceContext(Guid guidApplication, _ICompileContext comconReference, bool abortRunningThread);

		_ICompileContext GetReferenceContext(Guid guidApplication);

		void _SetPrecompileContext(_IPreCompileContext precom);

		void RemovePreCompCrossReferences(Guid guidObject);

		void LateLoadLibraryPreCompileContext(_IPreCompileContext precom);

		void ShowPrecomOptionChanged();

		void EndCompilation();

		string GetGlobalInitCode(ICompileContext3 comcon3, ISignature sign);

		bool RaiseAndCheckBeforeCompile(Guid guidApplication, IMessageCategory cmc);

		void OnBeforeMessageOutput(object sender, MessageOutputEventArgs e);

		void OnFilterMessageOutput(object sender, FilterMessageOutputEventArgs e);

		void OnAfterMessageOutput(object sender, MessageOutputEventArgs e);

		void OnSignatureChanged(Guid guidApplication, ISignature signOld, ISignature signNew, bool bSignificant);

		void OnSignatureDeleted(Guid guidApplication, ISignature signOld);

		void OnSignatureInserted(Guid guidApplication, ISignature signNew);

		void OnCompiledPOUDeleted(Guid guidApplication, ICompiledPOU signOld);

		void OnCompiledPOUInserted(Guid guidApplication, ICompiledPOU signNew);

		void OnCompiledPOUChanged(Guid guidApplication, _ICompiledPOU cpouOld, _ICompiledPOU cpouNew, bool bSignificant);

		void OnTaskConfigChanged(Guid guidApplication);

		void OnAddDownloadCode(AddImplicitCodeEventArgs e);

		void OnAddGlobalInitCode(AddImplicitCodeEventArgs e);

		void OnAddOnlineChangeCode(AddImplicitCodeEventArgs e);

		void OnAddLateLanguageModel(AddLanguageModelEventArgs e);

		void OnBeforeCompile(CompileEventArgs e);

		void OnCodeChanged(CodeChangeEventArgs e);

		void OnAfterCompile(AfterCompileEventArgs e);

		void OnAfterGenerateCode(AfterGenerateCodeEventArgs e);

		void OnBeforeLocation(CompileEventArgs e);

		void OnAfterLocation(CompileEventArgs e);

		void OnAfterGenerateGlobalInitCode(AfterGenerateGlobalInitEventArgs e);

		void OnBeforeGenerateCompiledCode(CompileEventArgs e);

		void OnBeforeGenerateRelinkCode(BeforeGenerateRelinkCodeEventArgs e);

		bool RaiseOnBeforeGenerateRelinkCode(Guid applicationGuid, _IOnlineChangeDetails ocd);

		void AddLibListForApp(Guid appGuid, _IPreCompileContext precom, ILMLibraryList2 liblist);

		void RemoveLibListForApp(Guid appGuid);

		ILMLibraryList2 GetLibListForApp(Guid appGuid, _IPreCompileContext precom);

		IEnumerable<ILMLibraryList2> GetLibListForApp(Guid appGuid);

		IEnumerable<IPreCompileContext> _AllPreCompileContexts(bool bWithDevices, bool bWithLibraries);

		IEnumerable<_IPreCompileContext> _AllApplicationPreCompileContexts();
	}
}
