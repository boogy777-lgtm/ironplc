using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEndDisassembler
	{
		void DisassembleCodeBuffer(Stream streamHelp, IBreakpointList bplist, ICompiledPOU cpou, TextWriter textwriter);
	}
}
