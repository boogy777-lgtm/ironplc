using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IParseTreeStreamProvider
	{
		Stream GetParseTreeStreamOfCompiledLibraryPOU(int projectHandle, Guid objectGuid);
	}
}
