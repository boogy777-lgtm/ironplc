using System;
using System.Collections.Generic;
using System.IO;
using CODESYS.Parser;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.Interfaces;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LibManObject;
using _3S.CoDeSys.OnlineExpressionInterpreter;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200001E RID: 30
	public interface IAPEnvironmentFacade
	{
		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000B8 RID: 184
		bool InjectionCompleted { get; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000B9 RID: 185
		LanguageModelManagerConsolidated LanguageModelMgr { get; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000BA RID: 186
		_ILMServiceProvider3 LMServiceProvider { get; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000BB RID: 187
		CompilerVersionManager CompilerVersionMgr { get; }

		// Token: 0x060000BC RID: 188
		IRegisteredTargetSetting GetTargetSetting(string path);

		// Token: 0x060000BD RID: 189
		ITargetSettings GetTargetSettingsById(IDeviceIdentification id);

		// Token: 0x060000BE RID: 190
		ITargetSettings GetSimulationTargetSettings(IDeviceIdentification deviceIdentification, Guid deviceGuid);

		// Token: 0x060000BF RID: 191
		void Information(string stMessage, string stMessageKey, params object[] messageArguments);

		// Token: 0x060000C0 RID: 192
		void Error(string stMessage, string stMessageKey, params object[] messageArguments);

		// Token: 0x060000C1 RID: 193
		void ErrorReportingWithDetails(string stMessage, EventHandler detailsClickHandler, EventArgs detailsClickArgs, string stMessageKey, params object[] messageArguments);

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000C2 RID: 194
		IMessageStorage MessageStorage { get; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000C3 RID: 195
		_IWarningHelper WarningHelper { get; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000C4 RID: 196
		Profile Profile { get; }

		// Token: 0x060000C5 RID: 197
		IOptionKey GetProjectOptionsRootKey(int nProjectHandle);

		// Token: 0x060000C6 RID: 198
		bool IsLoadProjectFinished(int nProjectHandle);

		// Token: 0x060000C7 RID: 199
		void FinishLoadProject(int nProjectHandle);

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x060000C8 RID: 200
		// (remove) Token: 0x060000C9 RID: 201
		event ProjectLoadFinishedEventHandler ProjectLoadFinished;

		// Token: 0x060000CA RID: 202
		bool ExistProject(int iProjectHandle);

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000CB RID: 203
		bool ExistsPrimaryProject { get; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000CC RID: 204
		int PrimaryProjectHandle { get; }

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000CD RID: 205
		string PrimaryProjectPath { get; }

		// Token: 0x060000CE RID: 206
		bool DoesPrimaryProjectExist(out int iPrimaryProjectHandle);

		// Token: 0x060000CF RID: 207
		IProject GetProjectByHandle(int nProjectHandle);

		// Token: 0x060000D0 RID: 208
		IProject GetProjectByLibraryId(string stLibraryId);

		// Token: 0x060000D1 RID: 209
		LibraryInfo GetLibraryInfo(string stLibraryId);

		// Token: 0x060000D2 RID: 210
		bool IsProjectInfoObjectBoolFlagSet(int iProjectHandle, string stKey);

		// Token: 0x060000D3 RID: 211
		bool IsProjectInfoObjectBoolFlagSet(string stLibraryId, string stKey);

		// Token: 0x060000D4 RID: 212
		string[] GetAuxiliaryFileEntries(int nProjectHandle);

		// Token: 0x060000D5 RID: 213
		void CalculateChecksumOfProject(int nProjectHandle, CRCSum projectCrc);

		// Token: 0x060000D6 RID: 214
		IEnumerable<int> GetAllProjectsWithAttribute(Guid projectAttr);

		// Token: 0x060000D7 RID: 215
		IProject[] GetProjectsByAttributes(params Guid[] projectAttrs);

		// Token: 0x060000D8 RID: 216
		bool ExistsObject(int nProjectHandle, Guid objectGuid);

		// Token: 0x060000D9 RID: 217
		string GetObjectName(int nProjectHandle, Guid objectGuid);

		// Token: 0x060000DA RID: 218
		IObjectProperty GetObjectProperty(int nProjectHandle, Guid objectGuid, Guid propertyGuid);

		// Token: 0x060000DB RID: 219
		bool CanDisassembleObject(int nProjectHandle, Guid objectGuid);

		// Token: 0x060000DC RID: 220
		IChildOnlineApplicationObject GetChildOnlineApplicationObject(int nProjectHandle, Guid guidAppl);

		// Token: 0x060000DD RID: 221
		IDeviceIdentification GetDeviceIdentification(int nProjectHandle, Guid guidDevice);

		// Token: 0x060000DE RID: 222
		IUndoManager GetUndoManager(int nProjectHandle);

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000DF RID: 223
		Guid ActiveApplicationGuid { get; }

		// Token: 0x060000E0 RID: 224
		Guid GetWorkspaceObjectGuid(int iProjectHandle);

		// Token: 0x060000E1 RID: 225
		Guid GetOnlineApplicationDeviceGuid(int iProjectHandle, Guid appGuid);

		// Token: 0x060000E2 RID: 226
		bool ExistsAuxiliaryFileEntry(int nProjectHandle, string stName);

		// Token: 0x060000E3 RID: 227
		void PutAuxiliaryFileEntry(int nProjectHandle, string stName, Stream readableStream);

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000E4 RID: 228
		IOEMCustomization OEMCustomization { get; }

		// Token: 0x060000E5 RID: 229
		IOnlineExpressionInterpreter3 CreateOnlineExpressionInterpreter();

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000E6 RID: 230
		_ICompileOptions2 CompileOptions { get; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000E7 RID: 231
		_ILibraryDevelopmentOptions LibraryDevelopmentOptions { get; }

		// Token: 0x060000E8 RID: 232
		IOptionKey CreateSubKey(OptionRoot root, string stSubKey);

		// Token: 0x060000E9 RID: 233
		IOptionKey OpenSubKey(OptionRoot root, string stSubKey);

		// Token: 0x060000EA RID: 234
		void DeleteSubKey(OptionRoot root, string stSubKey);

		// Token: 0x060000EB RID: 235
		void InvokeInPrimaryThread(Delegate dlgt, object[] args, bool bAsync);

		// Token: 0x060000EC RID: 236
		IProgressCallback StartLengthyOperation();

		// Token: 0x060000ED RID: 237
		IOnlineVarRef CreateWatch(IVarRef varRef);

		// Token: 0x060000EE RID: 238
		void GetAuxiliaryFileEntry(int nProjectHandle, string stName, Stream writableStream);

		// Token: 0x060000EF RID: 239
		bool IsSimulationMode(Guid gdApplication);

		// Token: 0x060000F0 RID: 240
		bool IsExcludedFromBuild(int projectHandle, Guid objectGuid, out bool inherited);

		// Token: 0x060000F1 RID: 241
		IMemorySettingsProvider GetMemorySettingsProvider(int iProjectHandle, Guid gdMemorySettingsprovider);

		// Token: 0x060000F2 RID: 242
		ILanguageModelProvider GetLanguageModelProvider(int iProjectHandle, Guid gdObject);

		// Token: 0x060000F3 RID: 243
		ICodegenerator CreateCodegenerator(Guid typeGuid);

		// Token: 0x060000F4 RID: 244
		IEnumerable<_ICompilerVersionsProvider> CreateCompilerVersionsProviders();

		// Token: 0x060000F5 RID: 245
		IEnumerable<IAttributeProvider> CreateAttributeProviders();

		// Token: 0x060000F6 RID: 246
		IArchiveReader CreateArchiveReader(Guid typeGuid);

		// Token: 0x060000F7 RID: 247
		IArchiveReader CreateBinaryArchiveReader();

		// Token: 0x060000F8 RID: 248
		IArchiveReader CreateNewBinaryArchiveReader();

		// Token: 0x060000F9 RID: 249
		IArchiveReader CreateEncryptedBinaryArchiveReader();

		// Token: 0x060000FA RID: 250
		IArchiveReader CreateNewEncryptedBinaryArchiveReader();

		// Token: 0x060000FB RID: 251
		IArchiveWriter CreateBinaryArchiveWriter();

		// Token: 0x060000FC RID: 252
		IArchiveWriter CreateNewBinaryArchiveWriter();

		// Token: 0x060000FD RID: 253
		IArchiveWriter CreateNewLowMemoryFootprintBinaryArchiveWriter();

		// Token: 0x060000FE RID: 254
		ISharedDataStorage GetSharedDataStorage(int nProjectHandle);

		// Token: 0x060000FF RID: 255
		bool ExecuteCleanAllCommand(params string[] batchArguments);

		// Token: 0x06000100 RID: 256
		void SaveAllEditors();

		// Token: 0x06000101 RID: 257
		string GetCommandLineOption(string stName);

		// Token: 0x06000102 RID: 258
		bool HasCommandLineSwitch(string stName);

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000103 RID: 259
		INamespaceConflictChecker NamespaceConflictCheckerOrNull { get; }

		// Token: 0x06000104 RID: 260
		object TryCreateResolver(Guid typeGuid);

		// Token: 0x06000105 RID: 261
		Stream GetParseTreeStreamOfCompiledLibraryPOU(int projectHandle, Guid objectGuid);

		// Token: 0x06000106 RID: 262
		bool IsLibraryPlaceholderResolutionExGuid(Guid resolverGuid);

		// Token: 0x06000107 RID: 263
		ILibraryPlaceholderResolutionEx CreateLibraryPlaceholderResolutionEx();

		// Token: 0x06000108 RID: 264
		IOnlineApplication GetOnlineApplication(Guid onlineApplicationGuid);

		// Token: 0x06000109 RID: 265
		string GetOverwrittenApplicationContextPath(Guid appObjectGuid);

		// Token: 0x0600010A RID: 266
		void RegisterEngineInitializedEventHandler(ParameterlessEventHandler eventHandler);

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x0600010B RID: 267
		// (remove) Token: 0x0600010C RID: 268
		event PrimaryProjectSwitchedEventHandler BeforePrimaryProjectSwitched;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x0600010D RID: 269
		// (remove) Token: 0x0600010E RID: 270
		event PrimaryProjectSwitchedEventHandler PrimaryProjectSwitched;

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x0600010F RID: 271
		// (remove) Token: 0x06000110 RID: 272
		event ProjectClosingEventHandler ProjectClosing;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x06000111 RID: 273
		// (remove) Token: 0x06000112 RID: 274
		event ProjectClosedEventHandler ProjectClosed;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x06000113 RID: 275
		// (remove) Token: 0x06000114 RID: 276
		event LoadLibrariesEventHandler AfterLoadingAllLibraries;

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x06000115 RID: 277
		// (remove) Token: 0x06000116 RID: 278
		event OptionEventHandler OptionCreated;

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x06000117 RID: 279
		// (remove) Token: 0x06000118 RID: 280
		event OptionEventHandler OptionChanged;

		// Token: 0x14000012 RID: 18
		// (add) Token: 0x06000119 RID: 281
		// (remove) Token: 0x0600011A RID: 282
		event OptionEventHandler OptionDeleted;

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600011B RID: 283
		IFileSystemFacade FileSystem { get; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600011C RID: 284
		IProjectSideCarService ProjectSideCarService { get; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x0600011D RID: 285
		_IScannerParserProvider ScannerParserProvider { get; }

		// Token: 0x0600011E RID: 286
		bool IsWarningAsError(MessageId id);

		// Token: 0x0600011F RID: 287
		IEnumerable<IParserService> GetAllParserServices();

		// Token: 0x06000120 RID: 288
		IEnumerable<IScannerService> GetAllScannerServices();
	}
}
