using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities13 : IPreCompileUtilities12, IPreCompileUtilities11, IPreCompileUtilities10, IPreCompileUtilities9, IPreCompileUtilities8, IPreCompileUtilities7, IPreCompileUtilities6, IPreCompileUtilities5, IPreCompileUtilities4, IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		IEnumerable<ISignature2> GetImplementedInterfaces(ISignature2 sign, ILMPreCompileSet precom);
	}
}
