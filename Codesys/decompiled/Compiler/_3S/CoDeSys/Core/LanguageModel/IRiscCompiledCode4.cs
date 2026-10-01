using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscCompiledCode4 : IRiscCompiledCode3, IRiscCompiledCode2, IRiscCompiledCode, ICompiledCode, ICloneable
	{
		IBreakpoint PendingBreakpoint { get; }
	}
}
