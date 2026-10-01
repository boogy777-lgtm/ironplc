using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMWarningConfiguration2 : ILMWarningConfiguration
	{
		IEnumerable<int> WarningAsErrorSet { get; set; }
	}
}
