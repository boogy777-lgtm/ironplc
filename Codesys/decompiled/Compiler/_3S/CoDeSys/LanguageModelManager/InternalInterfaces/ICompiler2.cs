using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompiler2 : ICompiler
	{
		IEnumerable<_ICompilerMessage> GetPOUMessages(_ICompiledPOU cpou, bool bIncludeErrorStatements, bool bIncludeErrorStatementsInConditionalPragmas);
	}
}
