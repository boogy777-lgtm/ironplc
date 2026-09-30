using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscCompiledCode3 : IRiscCompiledCode2, IRiscCompiledCode, ICompiledCode, ICloneable
	{
		Stream CodeStream { get; }
	}
}
