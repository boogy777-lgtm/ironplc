using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IPrecompileScope3 : _IPrecompileScope2, _IPrecompileScope, IPrecompileScope6, IPrecompileScope5, IPrecompileScope4, IPrecompileScope3, IPrecompileScope2, IPrecompileScope
	{
		Guid RootApplicationGuid { get; }
	}
}
