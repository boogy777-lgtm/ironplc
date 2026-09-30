using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICompiledPOU : ICompiledPOU10, ICompiledPOU9, ICompiledPOU8, ICompiledPOU6, ICompiledPOU5, ICompiledPOU4, ICompiledPOU3, ICompiledPOU, ICompiledPOU7
	{
		IEnumerable<IBitWriteAccess> BitWriteAccesses { get; }

		IList<IDataLocation> TryCatchFPAddresses { get; }

		IList<int> TryCatchCodeAddresses { get; }

		new Guid MessageGuid { get; set; }

		new Guid ObjectGuid { get; set; }

		new uint Checksum { get; set; }

		new int SignatureId { get; set; }

		new string Name { get; set; }

		new int CodeGeneratorStackSize { get; set; }

		new int ScratchSize { get; set; }

		new int MaxParamSize { get; set; }

		_ICompilerMessage[] Messages { get; }

		string LibraryPath { get; set; }

		new long TimeStamp { get; set; }

		long ImplicitReturnPositionPos { get; }

		_ICompilerMessage[] PrecompileMessages { get; set; }

		int NumberOfStatements { get; set; }

		_IStatement GetParseTree();

		void SetParseTree(_IStatement parseTree);

		void ClearBitWriteAccesses();

		void AddBitWriteAccess(IBitWriteAccess bwa);

		int AddTryCatchFPAddress(IDataLocation datloc);

		[Obsolete("use AddTryCatchCodeAddressIndex")]
		int AddTryCatchCodeAddress(int iOffset);

		void AddTryCatchCodeAddressIndex(int iOffset, int iIndex);

		bool GetFlagInternal(InternalCompiledPOUFlags cpFlag);

		void SetFlagInternal(InternalCompiledPOUFlags cpFlag, bool bSetTrue);

		void Accept(IExprementVisitor visitor);

		_ICompilerMessage[] GetMessages(bool bWithPrecompiledErrors);

		void SetMessages(IList<_ICompilerMessage> messages);

		void UpdateTimeStamp();

		void UpdateChecksum();

		void SetBreakpointList(IBreakpointList bpl);

		_ICompiledPOU Duplicate();

		_ICompiledPOU CreateCompiledPOU();

		string GetFullName(_ICompileContext comcon);

		void SetPrecompileMessages(IList<_ICompilerMessage> messages);

		void DuplicateParseTreeForCompilation();
	}
}
