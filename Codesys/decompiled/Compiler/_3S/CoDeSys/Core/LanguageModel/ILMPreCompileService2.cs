using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileService2 : ILMPreCompileService
	{
		ISignature FindSignature(ELMPreCompileSetType ePreCompileSetTypesToConsider, Guid guidObject, out ILMPreCompileSet precom);

		ILMPreCompileTypifier CreatePreCompileTypifier(Guid guidApplication);
	}
}
