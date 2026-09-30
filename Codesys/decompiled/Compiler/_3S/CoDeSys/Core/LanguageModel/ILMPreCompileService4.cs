using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileService4 : ILMPreCompileService3, ILMPreCompileService2, ILMPreCompileService
	{
		ISignature FindSignature(int nProjectHandle, Guid guidObject, out IPreCompileContext preCompileContext);
	}
}
