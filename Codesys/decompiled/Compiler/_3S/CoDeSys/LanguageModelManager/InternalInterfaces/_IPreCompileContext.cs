using System;
using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IPreCompileContext : IPreCompileContext14, IPreCompileContext13, IPreCompileContext12, IPreCompileContext11, IPreCompileContext10, IPreCompileContext9, IPreCompileContext8, IPreCompileContext7, IPreCompileContext6, IPreCompileContext5, IPreCompileContext4, IPreCompileContext3, IPreCompileContext2, IPreCompileContext, ICompileContextCommon
	{
		Hashtable DefineTable { get; }

		Hashtable TargetDefineTable { get; }

		_ILibraryPlaceholder[] Placeholders { get; }

		bool PrecompiledLibrary { get; set; }

		new bool DeviceApplication { get; set; }

		bool Dirty { get; set; }

		bool MinimalSystem { get; }

		bool SavedWithUnicodeIdentifiers { get; }

		new string LibraryPath { get; set; }

		string LibraryId { get; }

		new string Namespace { get; set; }

		bool LinkAll { get; set; }

		bool LinkInSimulation { get; set; }

		bool OnlineChangeable { get; set; }

		bool IgnoreLinkAll { get; set; }

		string UnitTestingDefine { get; set; }

		new bool QualifiedAccessOnly { get; set; }

		new bool SystemApplication { get; set; }

		bool IsInterfaceLibrary { get; set; }

		new bool Support32BitOnly { get; set; }

		bool SupportDynamicMemory { get; set; }

		bool GenerateContent { get; set; }

		string OrgNamespace { get; }

		new Guid ApplicationGuid { get; set; }

		_ITaskList TaskList { get; }

		_ISlotPOUList SlotPOUs { get; }

		long TimeStamp { get; set; }

		IEnumerable<_ICompiledPOU> AllCompiledPOUs { get; }

		_ISignature this[string stName] { get; }

		_ISignature this[Guid objectGuid] { get; }

		KindOfContext KindOf { get; set; }

		IList<_ISignature> _GVLSignatures { get; }

		IList<_ISignature> _AllSignatures { get; }

		IList<_ISignature> AllFlat { get; }

		bool SimulationMode { get; }

		int TargetOutputSize { get; set; }

		int TargetInputSize { get; set; }

		int TargetMemorySize { get; set; }

		int TargetStaticSize { get; set; }

		ICaseInsensitiveDictionary<string> PlaceholderTable { get; }

		IList<IStaticMemorySegment> StaticMemorySegments { get; }

		int PointerSize { get; }

		void ResetTargetSettings();

		void AddDefines(string stCSVList);

		void AddDefines(string stCSVList, bool bIsTargetDefine);

		ISignature GetParentSignature(Guid objectGuid);

		void UpdateTimeStamp();

		void AddSignature(_ISignature sign, bool bNotify);

		void AddRelatedSignature(_ISignature signObject, _ISignature signImplicit);

		void RemoveSignature(_ISignature sign);

		bool CheckSignature(_ISignature sign);

		bool CheckPOUCode(_ICompiledPOU cpou);

		void AddCompiledPOU(_ICompiledPOU cpou, bool bNotify);

		void RemoveCompiledPOU(_ICompiledPOU cpou);

		void RemoveMessages(IMessageStorage2 messagestorage, IMessageCategory cmc, Guid guidObject);

		bool MessageOutput(IMessageStorage messagestorage, IMessageCategory cmc, Guid guidObject);

		bool MessageOutput(IMessageStorage messagestorage, IMessageCategory cmc);

		void Remove(Guid objectGuid);

		_ICompiledPOU GetPOU(Guid objectGuid);

		IList<_ISignature> _GetSubSignatures(Guid objectGuid);

		void ClearPlaceholderTable();

		void RemoveTimeStampOnlyObjects();

		void CalculateLinkIds();

		_ICompileContext CreateCompiledContext(_ICompileContext comconOld, _ICompileContext comconParent, bool bLinkAll, out IList<_ICompilerMessage> errors);

		bool IsSystemLibrary(string stLibraryId);

		bool PublishSymbols(_IPreCompileContext precomLib);

		bool QualifiedAccessOnlyLocal(_IPreCompileContext precomLib);

		ICaseInsensitiveDictionary<ICaseInsensitiveDictionary<IExpression>> GetParameterTableTable();

		ICaseInsensitiveDictionary<IExpression> ParameterTable(string stLibraryId);

		uint CalculateLibChecksum(ITargetSettings targetSettings, Guid applicationGuid, IDeviceIdentification devid);

		uint CalculateParameterTableChecksum(out bool bContainsTables);

		_IPreCompileContext GetLibraryByName(string stNamespace, ITargetSettings tarset, Guid appObjectGuid, IDeviceIdentification devid);

		string GetNameOfLibrary(_IPreCompileContext precom, ITargetSettings tarset, Guid appObjectGuid, IDeviceIdentification devid);

		string GetNameOfLibrary(_IPreCompileContext precom, Guid guidApplication);

		string GetNameOfLibrary(_IPreCompileContext precom, _ICompileContext comcon);

		IList<string> GetAllLocalLibraries();

		void GetAllVisibleLibraries(out IList<_IPreCompileContext> alLibraries, out ICaseInsensitiveDictionary<string> htNamespaces, bool bIncludeSystemLibraries);

		void GetAllVisibleLibraries(out IList<_IPreCompileContext> alLibraries, out ICaseInsensitiveDictionary<string> htNamespaces, bool bIncludeSystemLibraries, Guid guidApplication);

		ICollection<_IPreCompileContext> PublishedLibraries(Guid guidApplication);

		IPreCompileContext[] GetLibraryContexts(bool bIncludeSystemLibraries);

		IEnumerable<IPreCompileContext> GetLibraryContexts2(bool bIncludeSystemLibraries);

		IPreCompileContext[] GetLibraryContexts(bool bIncludeSystemLibraries, Guid guidApplication);

		IEnumerable<IPreCompileContext> GetLibraryContexts2(bool bIncludeSystemLibraries, Guid guidApplication);

		ICollection<IPreCompileContext> LibraryContextsWithResolvedPlaceholders(_ICompileContext comcon);

		IPrecompileScope CreatePrecompileScope2(_ISignature signIn);

		IPrecompileScope CreatePrecompileScope(Guid guidSignature, Guid guidApplication);

		bool HasByteSupport();

		bool TypeIsSupported(TypeClass tc);

		void Define(string stDefineIdent, string stValue);

		void Undefine(string stDefineIdent);

		ITargetSettings GetTargetSettings();

		_ILibraryTable _GetLibraryTable();

		_ILibraryTable _GetLibraryTable(Guid applicationGuid);

		void ClearLibraryReferences();

		void ClearStaticLibraryTables();

		void AddLibrary(string stLibraryId, string stNamespace, bool bPublishSymbols, bool bSystemLibrary);

		void AddLibrary(string stLibraryId, ICaseInsensitiveDictionary<IExpression> paramTable, string stNamespace, bool bPublishSymbols, bool bSystemLibrary, bool bQualifiedOnlyLocal);

		void AddLibraryPlaceholder(string stPlaceholder, ICaseInsensitiveDictionary<IExpression> paramTable, string stDefaultLibrary, string stNamespace, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, Guid guidResolver, bool bQualifiedOnly, Guid libManGuid, bool bOptional);

		void RemoveLibrary(string stLibraryIdToRemove);

		void AddLibraryParamTable(string stLibraryId, ICaseInsensitiveDictionary<IExpression> paramTable);

		_IPreCompileContext ResolveLibraryPlaceholder(ITargetSettings tarset, Guid appObjectGuid, ILibraryPlaceholder2 placeholder, IDeviceIdentification devid);

		_IPreCompileContext ResolveLibraryPlaceholder(ITargetSettings tarset, Guid appObjectGuid, ILibraryPlaceholder2 placeholder, IDeviceIdentification devid, out string stLibraryId);

		void AddStaticMemorySegments(IEnumerable<IStaticMemorySegment> segments);

		int[] GetSignsForGlobalVar(string name);

		void SetSignatureChecked(_ISignature sign);

		void SetPouChecked(_ICompiledPOU cpou);

		bool CalculateVariableSizes(IList<ISignature> signaturelist, IList<IVariable> varlist, out IList<int> sizes);

		void UpdatePointerSize();
	}
}
