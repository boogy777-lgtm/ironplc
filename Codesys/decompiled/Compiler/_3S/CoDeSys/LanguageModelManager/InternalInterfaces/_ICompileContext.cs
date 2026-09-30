using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICompileContext : ICompileContext21, ICompileContext20, ICompileContext19, ICompileContext18, ICompileContext17, ICompileContext16, ICompileContext15, ICompileContext14, ICompileContext13, ICompileContext12, ICompileContext11, ICompileContext10, ICompileContext9, ICompileContext8, ICompileContext7, ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		IList<_ISignature> AllSignatureList { get; }

		IList<_ISignature> AllFlat { get; }

		IList<_ISignature> AllGlobalSignatures { get; }

		IList<_ISignature> SuperGlobalSignatures { get; }

		_IDataManager DataManager { get; set; }

		IArea[] OnlineChangeAreas { get; set; }

		new _ICompileContext ParentContext { get; }

		_ISignature this[int nId] { get; }

		_ISignature this[Guid guidObject] { get; }

		_ISignature this[string stName] { get; }

		IMemoryAllocationCallback DSFCallback { get; set; }

		IList<_ISignature> POUSignatures { get; }

		IList<_ISignature> POUSignaturesEx { get; }

		IList<_ISignature> _GVLSignatures { get; }

		IList<_ISignature> _GVLSignaturesEx { get; }

		IList<_ICompiledPOU> CompiledPOUList { get; }

		ICompiledSymbolTables CompiledSymbolTables { get; set; }

		ISymbolTables SymbolTables { get; set; }

		uint ParameterTableChecksum { get; set; }

		uint LibCheckSum { get; set; }

		uint PoolLibCheckSum { get; set; }

		_IDirvarLocationTable DirvarLocationTable { get; }

		new bool ContainsOnlineChangeCode { get; set; }

		bool ContainsCopyCode { get; set; }

		bool SimulationMode { get; set; }

		uint MemorySettingsChecksum { get; set; }

		bool ContainsCode { get; set; }

		bool InFastOnlineChange { get; set; }

		bool RetainInCycle { get; }

		string RetainCycleTask { get; }

		bool DoPersistentCode { get; }

		bool TreatLRealAsReal { get; }

		bool TreatInt64AsInt32 { get; }

		bool NoDefaultInitialization { get; }

		bool NewVFTable { get; }

		bool GenerateDirectCalls { get; }

		bool SupportDynamicMemory { get; }

		bool SupportSystemApplication { get; }

		bool SystemApplication { get; }

		bool NoNewReferences { get; set; }

		IDeviceIdentification DeviceId { get; set; }

		_ISlotPOUList SlotPOUs { get; }

		_ITaskList TaskList { get; }

		bool MinimalSystem { get; }

		bool ConcurrentOnlineChange { get; }

		bool SimpleConcurrentOnlineChange { get; }

		_ILibraryTable _LibraryTable { get; }

		new long TimeStampContext { get; set; }

		new long TimeStampPool { get; set; }

		new ICodegenerator Codegenerator { get; set; }

		bool GenerateContent { get; }

		Guid CodeId { get; set; }

		Guid DataId { get; set; }

		Guid LastCodeId { get; set; }

		Guid LastDataId { get; set; }

		new uint CheckSumCode { get; set; }

		new uint CheckSumData { get; set; }

		uint CheckSumCodeLast { get; set; }

		uint CheckSumDataLast { get; set; }

		IList<_ILibInfo> PoolLibraryList { get; }

		IList<_IImplicitReferenceVariable> ImplicitReferenceVariables { get; }

		bool DeviceApplication { get; set; }

		IList<IStaticMemorySegment> StaticMemorySegments { get; }

		new bool bFastOnlineChange { set; }

		bool OnlineChangeSupported { get; }

		uint ProjectChecksum { get; set; }

		IDeviceSpecificProperties DeviceSpecificProperties { get; set; }

		bool IsUpToDate();

		_ICompileContext Duplicate();

		IList<_ISignature> GetAllSignaturesFlatInvariant();

		IList<_ICompiledPOU> _GetAllCompiledPOUs();

		ITargetSettings GetTargetSettings();

		IDeviceIdentification GetDeviceIdentification();

		void ConfigureMemory(_IMemorySettings memset, IList<_IArea> alAreas, int nFirstArea);

		bool IsUpToDate(_IPreCompileContext precomp, _IPreCompileContext precompPool, out bool bOnlineChangePossible);

		void SetPOUSignaturesEx(IList<_ISignature> signs);

		IList<_ISignature> GetLocalGVLSignatures(string stLibraryId);

		IList<_ISignature> GetLibraryGVLSignatures(string stLibraryId);

		_ISignature CreateCompiledSignature(_ISignature signPre, _ISignature signOld, _IPreCompileContext comconOrg, _ICompileContext comconOld);

		_ISignature AddCompiledSignature(_ISignature sign, _IPreCompileContext precomconOrg, _ICompileContext comconOld);

		_ISignature AddCompiledSignature(_ISignature sign, _IPreCompileContext precomconOrg, _ICompileContext comconOld, bool bImplicit);

		_ISignature AddCompiledSignature(_ISignature sign, string stName, _IPreCompileContext precomconOrg, _ICompileContext comconOld, bool bImplicit);

		bool TypeIsSupported(TypeClass tc);

		bool HasByteSupport();

		bool GetCodegeneratorProperty(CodegeneratorProperties cgpProperty);

		bool LibraryIsUnique(string stLibraryPath);

		bool LibraryIsUnique(_IPreCompileContext precomlib);

		_IPreCompileContext GetLibraryContextIgnoreVersion(_IPreCompileContext precom, _IPreCompileContext precomPool, string stLibraryPathIn);

		IPreCompileContext[] GetReferencedLibraries(string stLibraryId);

		void AddSignature(_ISignature sign, _ISignature signRef, _ICompileContext comconRef);

		void AddSignature(_ISignature sign, _ISignature signRef, _ICompileContext comconRef, bool bInternal);

		void RemoveSignature(_ISignature sign);

		void RemoveCompiledPOU(_ICompiledPOU cpou);

		void ReplaceSignature(_ISignature signOld, _ISignature signNew);

		_ICompiledPOU _GetCompiledPOUById(int nId);

		void AddCompiledPOU(_ICompiledPOU cpou, _ICompileContext comconRef);

		void AddCompiledPOU(_ICompiledPOU cpou, _ISignature sign, _ICompileContext comconRef);

		void AddCompiledPOU(_ICompiledPOU cpou, _ISignature sign, bool bImplicit, _ICompileContext comconRef);

		void AddCompiledPOUSimple(_ICompiledPOU cpou);

		[Obsolete("Das ist vermutlich nicht die Version die dich interessiert!")]
		Version GetDeviceVersion();

		void Define(string stDefineIdent, string stValue, bool bPrecompile);

		new void Define(string stDefineIdent, string stValue);

		new void Undefine(string stDefineIdent);

		bool DefineChanged(_IPreCompileContext precomp);

		string GetLocalLibraryNamespace(_IPreCompileContext precom);

		void AddAddressCrossReference(IDirectVariable dirvar, int nSignatureId, ISourcePosition sourcepos, AccessFlag access);

		void AddAddressCrossReference(IDirectVariable dirvar, int nSignatureId, IAddressCodePosition codepos);

		bool IsUpToDate(_IPreCompileContext precomp, _IPreCompileContext precompPool, out bool bOnlineChangePossible, out bool bFastOnlineChange);

		bool LibraryParamTablesEqual(_IPreCompileContext precom);

		bool LibraryListsEqual(_IPreCompileContext precomp, _IPreCompileContext precompPool);

		bool NeedsExternalFunctionCall(IOperatorExpression op, ICompiledType type, ref string stFunctionName, ref TypeClass tcWithType);

		bool NeedsExternalFunctionCall(IConversionExpression conv, ref string stFunctionName, ref TypeClass tcWithType);

		void ResetVarFlags(VarFlag fl);

		void CopyLibraryReferences(_ICompileContext comconOld);

		_ISignature GetInitFunction(_ISignature sign);

		ISignature FindSuperGlobalSignature(string stName);

		bool QualifiedAccessOnlyLocal(_IPreCompileContext precomLocal, _IPreCompileContext precomLib);

		_IPreCompileContext GetContextByLibraryPath(string stPath);

		_IPreCompileContext GetLibraryById(int nLibraryId);

		int GetIdOfLibraryReference(string stLibraryId, string stNamespace);

		int GetIdOfLibraryReference(_IPreCompileContext precom, string stNamespace);

		_IPreCompileContext GetLibraryByName(string stNamespace);

		bool ContainsLibraryReference(_IPreCompileContext precom, string stNamespace, bool bOutOfLibrary, bool bOutOfPool);

		string GetNameOfLibrary(int nLibraryId);

		void AddLibrary(string stPath, int nId, string stNamespace, bool bOutOfLibrary, bool bOutOfPool);

		void AddLibrary(_IPreCompileContext precomLib, _ICompileContext comconRef, string stNamespace, bool bOutOfLibrary, bool bOutOfPool);

		bool GetInterfaceCRC(_ISignature sign, out uint uiCRC);

		void ResetExprementHashTables();

		IDictionary<string, IUnresolvedPlaceholder> GetUnresolvedPlaceholderTable();

		IFlowVarRef GetFlowPositionBySourcePostion(_IExpression expToFind, int nSignatureId, ISourcePosition sourcepos, string stInstancePath, IVarRef varrefInstance);

		IEnumerable<IFlowVarRef> GetAllFlowPositions(int nSignatureId, long[] alPositionsOfInterest, string stInstancePath, IVarRef varrefInstance);

		string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace);

		void AddImplicitReferenceVariable(_ISignature sign, _IVariable var);

		uint CalculateProjectChecksum();

		IEnumerable<IChangedLMObject> FindChangedObjectsDetailed();

		void RemoveWatchVariable(string stName);
	}
}
