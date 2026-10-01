using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMConfigurationService
	{
		IEnumerable<IAttribute> RegisteredAttributes { get; }

		ILMWarningConfiguration WarningConfiguration { get; }

		ILMCompileOptions CompileOptions { get; }

		ILMLibraryDevelopmentOptions LibraryDevelopmentOptions { get; }

		bool ExecutionpointLoggingEnabled(Guid appGuid);
	}
}
