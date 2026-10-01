using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscCompiledCode2 : IRiscCompiledCode, ICompiledCode, ICloneable
	{
		IList<IRiscCompiledCodeEntry> InstructionList { get; }

		IDictionary<string, IRiscLabel> LabelTable { get; }
	}
}
