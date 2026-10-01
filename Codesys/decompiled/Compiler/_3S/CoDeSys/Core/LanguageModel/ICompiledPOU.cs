using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledPOU
	{
		IStatement ParseTree { get; }

		IBreakpointList BreakpointList { get; }

		string Name { get; }

		ICompiledCode CompiledCode { get; set; }

		int SignatureId { get; }

		int ScratchSize { get; }

		int MaxParamSize { get; }

		[Obsolete("The timestamp is still valid, but better use ICompiledPOU3.Checksum")]
		long TimeStamp { get; }

		ISourcePosition ImplicitReturnPosition { get; }

		bool GetFlag(CompiledPOUFlags cpFlag);

		ISourcePosition GetSourcePositionOfBreakpoint(IBreakpoint bp);
	}
}
