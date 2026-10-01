using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMWarningConfiguration
	{
		IEnumerable<int> WarningsSet { get; }

		IEnumerable<int> DisabledWarningsSet { get; set; }

		string GetLocalizedWarningText(int iWarningId);
	}
}
