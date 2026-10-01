using System;
using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ICompileContextSerializable
	{
		Guid ApplicationGuid { get; }

		bool bFastOnlineChange { get; set; }

		bool LoadWithoutTargetsettings { get; set; }

		Guid CodegeneratorGuid { get; set; }

		KindOfContext KindOf { get; set; }

		IEnumerable<ISignatureSerializable> GlobalSignaturesSerializable { get; }

		IEnumerable<ISignatureSerializable> SignaturesSerializable { get; }

		IEnumerable<ICompiledPOUSerializable> CompiledPOUsSerializable { get; }

		int SerializableSignatureIdManager { get; set; }

		int SerializableLibraryIdManager { get; set; }

		Hashtable DefineTable { get; }

		Hashtable PrecompileDefineTable { get; }

		ICompileOptionsSerializable CompileOptionsSerializable { get; set; }

		bool SupportDynamicMemory { get; set; }

		bool GenerateContent { get; set; }

		_ITaskList TaskList { get; set; }

		IList<IStaticMemorySegment> StaticMemorySegments { get; set; }

		_ISlotPOUList SlotPOUs { get; set; }

		_ILibraryTable _LibraryTable { get; set; }

		IList<_IImplicitReferenceVariable> ImplicitReferenceVariables { get; set; }

		bool DeviceApplication { get; set; }

		long TimeStampContext { get; set; }

		long TimeStampPool { get; set; }

		bool ContainsOnlineChangeCode { get; set; }

		Guid CodeId { get; set; }

		Guid DataId { get; set; }

		Guid LastCodeId { get; set; }

		Guid LastDataId { get; set; }

		uint CheckSumCode { get; set; }

		uint CheckSumData { get; set; }

		uint CheckSumCodeLast { get; set; }

		uint CheckSumDataLast { get; set; }

		uint MemorySettingsChecksum { get; set; }

		_IDataManager DataManager { get; set; }

		uint PrecompileContextNamesChecksum { get; set; }

		uint ParameterTableChecksum { get; set; }

		uint LibCheckSum { get; set; }

		uint PoolLibCheckSum { get; set; }

		bool SimulationMode { get; set; }

		bool ContainsCode { get; set; }

		uint ProjectChecksum { get; set; }

		IDeviceIdentification DeviceId { get; set; }

		void SetGlobalSignatures(IEnumerable<_ISignature> signs);

		void SetSignatures(IEnumerable<_ISignature> signs);

		void SetCompiledPOUs(IEnumerable<_ICompiledPOU> pous);

		void AfterDeserialize();

		void Define(string stDefineIdent, string stValue, bool bPrecompile);

		void AddAddressCrossReference(IDirectVariable dirvar, int nSignatureId, IAddressCodePosition codepos);

		IDirectVariableCrossRefTable GetDirectVariableTable();
	}
}
