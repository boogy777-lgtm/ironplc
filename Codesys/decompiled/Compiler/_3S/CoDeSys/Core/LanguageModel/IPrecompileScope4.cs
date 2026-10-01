using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompileScope4 : IPrecompileScope3, IPrecompileScope2, IPrecompileScope
	{
		Guid ApplicationGuid { get; set; }
	}
}
