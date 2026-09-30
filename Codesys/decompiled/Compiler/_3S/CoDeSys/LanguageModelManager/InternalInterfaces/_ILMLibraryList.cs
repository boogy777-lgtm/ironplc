using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILMLibraryList : ILMLibraryList2, ILMLibraryList
	{
		IEnumerable<ILMLibraryInfo> AllLibraries { get; }
	}
}
