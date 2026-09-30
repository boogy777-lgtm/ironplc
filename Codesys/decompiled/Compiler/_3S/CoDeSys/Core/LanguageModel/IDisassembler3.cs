using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDisassembler3 : IDisassembler2, IDisassembler
	{
		bool DisassembleCode(ICompileContext12 comcon, ICompiledCode2 code, IBreakpointList2 bplist, ICompiledPOU5 cpou, TextWriter textwriter);
	}
}
