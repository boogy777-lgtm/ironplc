using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscCompiledCode5 : IRiscCompiledCode4, IRiscCompiledCode3, IRiscCompiledCode2, IRiscCompiledCode, ICompiledCode, ICloneable
	{
		void DefineBeforeCallBreakpoint(ICallExpression call);
	}
}
