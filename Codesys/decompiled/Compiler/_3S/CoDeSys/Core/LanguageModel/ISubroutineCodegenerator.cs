using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISubroutineCodegenerator
	{
		void Generate(ICompiledPOU cpou, ISequenceStatement seqMainRoutine, bool bKeepCompileInformation, IEnumerable<ISubroutineStatement> subroutines);
	}
}
