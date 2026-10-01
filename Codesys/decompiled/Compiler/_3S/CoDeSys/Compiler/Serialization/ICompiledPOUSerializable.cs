using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ICompiledPOUSerializable
	{
		IEnumerable<IBitWriteAccess> BitWriteAccesses { get; set; }

		IList<IDataLocation> TryCatchFPAddresses { get; set; }

		IList<int> TryCatchCodeAddresses { get; set; }

		IBreakpointList BreakpointList { get; }

		Guid ObjectGuid { get; set; }

		Guid ParentObjectGuid { get; set; }

		Guid MessageGuid { get; set; }

		CompiledPOUFlags Flags { get; set; }

		InternalCompiledPOUFlags InternalFlags { get; set; }

		string LibraryPath { get; set; }

		uint Checksum { get; set; }

		long TimeStamp { get; set; }

		int SignatureId { get; set; }

		int ScratchSize { get; set; }

		int MaxParamSize { get; set; }

		ICompiledCode CompiledCode { get; set; }

		int CodeGeneratorStackSize { get; set; }

		_ICompilerMessage[] PrecompileMessages { get; set; }

		string Name { get; set; }

		string OriginalName { get; }

		_ICompilerMessage[] Messages { get; }

		void SetBreakpointList(IBreakpointList breakpointList);

		void AfterDeserialize();

		void SetParseTree(_IStatement parseTree);

		void SetMessages(IList<_ICompilerMessage> messages);
	}
}
