using System;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface ILanguageVersionDependentService
	{
		Version LanguageVersion { get; }
	}
}
