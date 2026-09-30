using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompileService
	{
		IEnumerable<ILMCompiledApplicationSet> CompiledApplicationSets { get; }

		event EventHandler<BeforeGenerateRelinkCodeEventArgs> BeforeGenerateRelinkCode;

		event EventHandler<AfterGenerateGlobalInitEventArgs> AfterGenerateGlobalInitCode;

		event EventHandler<CompileEventArgs> BeforeGenerateCompiledCode;

		event BeforeMessageOutputEventHandler BeforeMessageOutput;

		event FilterMessageOutputEventHandler FilterMessageOutput;

		event AfterMessageOutputEventHandler AfterMessageOutput;

		event CompileEventHandler BeforeCompile;

		event CompileEventHandler AfterCompile;

		event CompileEventHandler CodeChanged;

		event AddLanguageModelEventHandler AddLateLanguageModel;

		event CompileEventHandler BeforeLocation;

		event CompileEventHandler AfterLocation;

		event CompileEventHandler AfterGenerateCode;

		event AddImplicitCodeEventHandler AddDownloadCode;

		event AddImplicitCodeEventHandler AddGlobalInitCode;

		event AddImplicitCodeEventHandler AddOnlineChangeCode;

		ILMCompiledApplicationSet GetCompiledApplicationSet(Guid guidApplication);

		ILMCompiledApplicationTypification GetTypificator(Guid guidApplication);

		ILMCompiledApplicationQuery QueryCompiledApplicationSet(Guid guidApplication);

		ILMCompiledApplicationDebugging GetApplicationDebugger(Guid guidApplication);

		ILMCompiledApplicationContentDumper GetCompiledApplicationContentDumper(Guid guidApplication);

		Guid GetCompiledApplicationSetGuidByName(string stResourceName, string stApplicationName);

		IDownloadInfo GetDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject);

		void GetCompiledIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId);

		bool IsUpToDate(Guid guidApplication);

		bool IsUpToDate(Guid guidApplication, out bool bOnlineChangePossible);

		IEnumerable<IExternalReference> GetExternalReferences(Guid guidApplication, bool bCompactDownload);

		IAddressCalculation CreateAddressCalculaton(Guid guidApplication);

		[Obsolete("GetAreaSizeAfterShrinking should no longer be used. Use GetDownloadInfo instead. IDownloadInfo.Areas contains already shrinked area sizes.")]
		ulong[] GetAreaSizesAfterShrinking(Guid appGuid);

		IApplicationContent BuildApplicationContentFromUpload(byte[] bytes);

		IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder);

		IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder, bool bByteSupport);

		IDownloadInfo GetOfflineBootProjectInfo(Guid guidApplication);

		IDownloadInfo GetOnlineBootProjectInfo(Guid guidApplication);

		IVariable GetVariable(string stVariableName);

		IEnumerable<ITaskCrossref> GetTaskReferencesOfInstancePath(string stInstancePath);

		IType GetTypeOfLiteral(ILiteralExpression litExp, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32, bool bUnicodeNotSupported);

		IType GetTypeOfLiteral(ILiteralExpression litExp, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32);

		string GetCompilerDefinesOfDeviceDescription(Guid guidApplication);
	}
}
