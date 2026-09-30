using System;
using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ISignature : ISignature7, ISignature6, ISignature5, ISignature4, ISignature3, ISignature2, ISignature
	{
		new string Name { get; set; }

		IList<IVariable> AllLazy { get; }

		IList<IVariable> AllExternals { get; }

		IList<IVariable> AllConstants { get; }

		IList<IVariable> AllRetains { get; }

		IList<IVariable> AllNonRetains { get; }

		IList<IVariable> AllForInitCode { get; }

		IList<IVariable> NonReplacedConstants { get; }

		IInterfaceHierarchy InterfaceHierarchy { get; set; }

		_IExpression _BaseSignature { get; set; }

		IEnumerable _SubSignatures { get; }

		new IDataLocation FPDataLocation { get; set; }

		new int Size { get; set; }

		new int CalleeSize { get; set; }

		int HighestUsedOffset { get; set; }

		_IExpression _NameExpression { get; set; }

		new string Comment { get; set; }

		string DocuComment { get; set; }

		int NextId { get; }

		new Guid ParentObjectGuid { get; set; }

		new uint Checksum { get; set; }

		new uint ChecksumNoInit { get; set; }

		new Guid MessageGuid { get; set; }

		string LibraryId { get; }

		new int ParentSignatureId { get; set; }

		new string LibraryPath { get; set; }

		new Operator POUType { get; set; }

		bool HasErrors { get; }

		int[] AllUsedIds { get; }

		int[] ReferencerIds { get; }

		IList<uint> CalleeIdList { get; }

		bool IsLibraryObject { get; }

		bool IsSourceLibraryObject { get; }

		[Obsolete("The timestamp is still valid, but better use ISignature3.Checksum")]
		new long TimeStamp { get; set; }

		uint CRC { get; }

		new int PrecompileId { get; set; }

		new int PrecompileParentId { get; set; }

		new int PrecompileBaseSignatureId { get; set; }

		IList<_ICompilerMessage> PrecompileMessages { get; set; }

		IList<_IVariable> AllVariables { get; }

		bool HasMemoryReserve { get; }

		_ILMEntity RawDeclaration { get; set; }

		_ISignature Duplicate(bool bDeep);

		void AddMessage(IMessage4 message);

		void AddMessage(Severity severity, MessageId mid, params object[] args);

		void AddError(IMessage cm);

		void AddMessageString(_ISourcePosition sourcepos, Severity severity, string stErrorString, params object[] args);

		void AddMessage(_ISourcePosition sourcepos, Severity severity, MessageId mid, params object[] args);

		void AddWarning(IToken tokenPos, string stWarning, MessageId mid);

		void AddWarning(ISourcePosition sourcePosition, string stWarning, MessageId mid);

		void AddError(IToken tokenPos, string stError, Guid guidMessage, MessageId mid);

		void AddError(IToken tokenPos, string stError, MessageId mid);

		void AddMessages(IList<_ICompilerMessage> cm);

		void AddMessages(ICollection cm);

		IMessage4 CreateCompilerMessage(ISourcePosition sp, string stError, Severity severity, int id);

		void ResetBaseSignatureId();

		void SetFlag(SignatureFlag sf, bool bSet);

		void SetBaseSignatureId(int id);

		void SetInterfaceIds(int[] ids);

		void AddPrecompileInterfaceId(int id);

		void AddAttribute(string stAttribute, string stValue);

		_ISignature[] GetSubSignatures();

		_ISignature[] GetOrderedSubSignatures();

		bool InsertVariable(_IVariable varin, int i);

		bool AddVariable(_IVariable var);

		bool RemoveVariable(_IVariable var);

		void AddInterface(_IExpression expInterface);

		string GetSearchName(_ICompileContext cc);

		_ISignature CreateCompiledSignature(_ISignature signOld, bool bByteSupport);

		_ISignature CreateCompiledSignature(_ISignature signOld, _ICompileContext comcon, _ICompileContext comconOld, bool bByteSupport);

		void AddDeclarer(int nId);

		void AddReferencer(int nId);

		_IType ReplaceTypes(_IType type, SpecialFeatures sf);

		bool GetFlagInternal(SignatureFlagInternal sfFlag);

		void SetFlagInternal(SignatureFlagInternal sfFlag, bool bSetTrue);

		bool AddSubSignature(ISignature sign);

		bool RemoveSubSignature(ISignature sign);

		bool GetAttributeIntValue(string stAttribute, ref int nValue);

		void AddCaller(int nId);

		void SetCalleeIds(IList<uint> calleeIds);

		void AddUsed(int nId);

		void AddCallee(int nId, bool bVirtual);

		bool IsEqualCompile(_ISignature signInX, bool bCompareInitValues);

		bool InstanceLocalsChanged(_ISignature signRef);

		void AddTaskReference(byte byTaskIndex);

		void CreateVirtualFunctionTable(_ICompileContext comcon);

		void UpdateTimeStamp();

		void RegisterPrecompileVariable(_IVariable var);

		void AddPrecompileDeclarer(int nPrecompileId);

		void AddPrecompileCaller(int nPrecompileId);

		void AddPrecompileCallee(int nPrecompileId, bool bVirtual);

		IList<_ICompilerMessage> GetMessages(bool bWithPrecompileErrors);

		bool HasFlag(SignatureFlag flag);

		int GetSizeOfMemoryReserve();
	}
}
