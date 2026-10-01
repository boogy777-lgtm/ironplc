using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCreatorService3 : ILMCreatorService2, ILMCreatorService
	{
		IRawSTParser CreateRawSTParser(string stText, bool bImplicit, Version version, ILMCompileOptions3 compileOptions);

		IRawSTParser CreateRawSTParser(char[] stText, bool bImplicit, Version version, ILMCompileOptions3 compileOptions);
	}
}
