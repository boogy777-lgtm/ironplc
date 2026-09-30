using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompiler4 : ICompiler3, ICompiler2, ICompiler
	{
		_IScanner CreateScanner(Version version);

		_IParser CreateParser(IScanner scanner, bool bImplicit, Version version, ILMCompileOptions3 compileOptions);
	}
}
