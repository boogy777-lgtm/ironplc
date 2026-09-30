using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryTable2 : ILibraryTable
	{
		IEnumerable<string> AllReferencedLibraries();
	}
}
